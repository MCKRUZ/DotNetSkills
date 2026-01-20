using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SkillsCore.Config;
using SkillsCore.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SkillsCore.Services;

/// <summary>
/// Implementation of skill loading with progressive disclosure and caching.
/// </summary>
/// <remarks>
/// Key implementation patterns:
/// - Frontmatter is parsed using regex to separate YAML from markdown
/// - Two-level caching: metadata cache and full skill cache
/// - Resources are discovered (file paths captured) but not loaded until requested
/// </remarks>
public partial class SkillLoaderService : ISkillLoader
{
    private readonly SkillsConfig _config;
    private readonly IDeserializer _yamlDeserializer;

    // Two-level caching for progressive disclosure
    private readonly ConcurrentDictionary<string, SkillDefinition> _metadataCache = new();
    private readonly ConcurrentDictionary<string, SkillDefinition> _fullCache = new();

    private DateTime _lastDiscovery = DateTime.MinValue;
    private readonly SemaphoreSlim _discoveryLock = new(1, 1);

    // Regex pattern to extract YAML frontmatter from markdown
    // Matches: ---\n<yaml content>\n---\n<markdown body>
    [GeneratedRegex(@"^---\s*\n([\s\S]*?)\n---\s*\n([\s\S]*)", RegexOptions.Compiled)]
    private static partial Regex FrontmatterRegex();

    public SkillLoaderService(IOptions<SkillsConfig> options)
    {
        _config = options.Value;

        // Configure YAML deserializer with camelCase naming (matches typical frontmatter style)
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SkillDefinition>> DiscoverSkillsAsync(CancellationToken ct = default)
    {
        // Check if cache is still valid
        if (IsCacheValid() && _metadataCache.Count > 0)
        {
            return _metadataCache.Values.ToList();
        }

        await _discoveryLock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
            if (IsCacheValid() && _metadataCache.Count > 0)
            {
                return _metadataCache.Values.ToList();
            }

            // Clear existing caches
            _metadataCache.Clear();
            _fullCache.Clear();

            var basePath = GetAbsoluteBasePath();
            if (!Directory.Exists(basePath))
            {
                return Array.Empty<SkillDefinition>();
            }

            // Find all SKILL.md files
            var skillFiles = Directory.GetFiles(basePath, _config.SkillFileName, SearchOption.AllDirectories);

            foreach (var skillFile in skillFiles)
            {
                ct.ThrowIfCancellationRequested();

                var skill = await LoadSkillMetadataAsync(skillFile, ct);
                if (skill != null)
                {
                    _metadataCache.TryAdd(skill.Id, skill);
                }
            }

            _lastDiscovery = DateTime.UtcNow;
            return _metadataCache.Values.ToList();
        }
        finally
        {
            _discoveryLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<SkillDefinition?> LoadSkillAsync(string skillId, CancellationToken ct = default)
    {
        // Check full cache first
        if (_fullCache.TryGetValue(skillId, out var cachedFull) && IsCacheValid())
        {
            return cachedFull;
        }

        // Ensure discovery has run
        await DiscoverSkillsAsync(ct);

        // Get metadata version
        if (!_metadataCache.TryGetValue(skillId, out var metadata))
        {
            return null;
        }

        // Load full skill
        var fullSkill = await LoadSkillFullAsync(metadata.FilePath, ct);
        if (fullSkill != null)
        {
            _fullCache.TryAdd(skillId, fullSkill);
        }

        return fullSkill;
    }

    /// <inheritdoc />
    public async Task<string?> LoadResourceContentAsync(SkillResource resource, CancellationToken ct = default)
    {
        // Return cached content if already loaded
        if (resource.IsLoaded && resource.Content != null)
        {
            return resource.Content;
        }

        // Verify file exists
        if (!File.Exists(resource.FilePath))
        {
            return null;
        }

        // Load content
        resource.Content = await File.ReadAllTextAsync(resource.FilePath, ct);
        resource.IsLoaded = true;

        return resource.Content;
    }

    /// <inheritdoc />
    public async Task<SkillDefinition?> GetSkillMetadataAsync(string skillId, CancellationToken ct = default)
    {
        // Check caches
        if (_fullCache.TryGetValue(skillId, out var full))
        {
            return full;
        }

        if (_metadataCache.TryGetValue(skillId, out var metadata))
        {
            return metadata;
        }

        // Trigger discovery and try again
        await DiscoverSkillsAsync(ct);
        _metadataCache.TryGetValue(skillId, out metadata);

        return metadata;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SkillDefinition>> FindSkillsByTagAsync(string tag, CancellationToken ct = default)
    {
        var skills = await DiscoverSkillsAsync(ct);
        return skills
            .Where(s => s.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    /// <inheritdoc />
    public void InvalidateCache()
    {
        _metadataCache.Clear();
        _fullCache.Clear();
        _lastDiscovery = DateTime.MinValue;
    }

    // ═══════════════════════════════════════════════════════════════════
    // Private Helper Methods
    // ═══════════════════════════════════════════════════════════════════

    private bool IsCacheValid()
    {
        return DateTime.UtcNow - _lastDiscovery < _config.CacheDuration;
    }

    private string GetAbsoluteBasePath()
    {
        var basePath = _config.BasePath;

        if (Path.IsPathRooted(basePath))
        {
            return basePath;
        }

        // Resolve relative to application base directory
        return Path.Combine(AppContext.BaseDirectory, basePath);
    }

    /// <summary>
    /// Load only metadata (frontmatter) from a skill file - Level 1 progressive disclosure.
    /// </summary>
    private async Task<SkillDefinition?> LoadSkillMetadataAsync(string filePath, CancellationToken ct)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, ct);
            var (frontmatter, _) = ParseFrontmatter(content);

            if (frontmatter == null)
            {
                return null;
            }

            var baseDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
            var skillId = Path.GetFileName(baseDirectory);
            var fileInfo = new FileInfo(filePath);

            var skill = new SkillDefinition
            {
                Id = skillId,
                Name = GetFrontmatterValue(frontmatter, "name") ?? skillId,
                Description = GetFrontmatterValue(frontmatter, "description") ?? string.Empty,
                Version = GetFrontmatterValue(frontmatter, "version"),
                Author = GetFrontmatterValue(frontmatter, "author"),
                Category = GetFrontmatterValue(frontmatter, "category"),
                Tags = GetFrontmatterList(frontmatter, "tags"),
                FilePath = filePath,
                BaseDirectory = baseDirectory,
                LoadedAt = DateTime.UtcNow,
                LastModified = fileInfo.LastWriteTimeUtc,
                IsFullyLoaded = false,
                Metadata = frontmatter
            };

            // Discover resources (without loading content) for resource counts
            DiscoverResources(skill);

            return skill;
        }
        catch (Exception)
        {
            // Skip skills that fail to parse
            return null;
        }
    }

    /// <summary>
    /// Load full skill including instructions - Level 2 progressive disclosure.
    /// </summary>
    private async Task<SkillDefinition?> LoadSkillFullAsync(string filePath, CancellationToken ct)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, ct);
            var (frontmatter, body) = ParseFrontmatter(content);

            if (frontmatter == null)
            {
                return null;
            }

            var baseDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
            var skillId = Path.GetFileName(baseDirectory);
            var fileInfo = new FileInfo(filePath);

            var skill = new SkillDefinition
            {
                Id = skillId,
                Name = GetFrontmatterValue(frontmatter, "name") ?? skillId,
                Description = GetFrontmatterValue(frontmatter, "description") ?? string.Empty,
                Instructions = body?.Trim(),  // Include the markdown body
                Version = GetFrontmatterValue(frontmatter, "version"),
                Author = GetFrontmatterValue(frontmatter, "author"),
                Category = GetFrontmatterValue(frontmatter, "category"),
                Tags = GetFrontmatterList(frontmatter, "tags"),
                FilePath = filePath,
                BaseDirectory = baseDirectory,
                LoadedAt = DateTime.UtcNow,
                LastModified = fileInfo.LastWriteTimeUtc,
                IsFullyLoaded = true,  // Mark as fully loaded
                Metadata = frontmatter
            };

            // Discover resources
            DiscoverResources(skill);

            // Optionally eager-load resource contents
            if (_config.EagerLoadResources)
            {
                await LoadAllResourceContentsAsync(skill, ct);
            }

            return skill;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Parse YAML frontmatter from markdown content.
    /// </summary>
    /// <returns>Tuple of (frontmatter dictionary, markdown body).</returns>
    private (Dictionary<string, object>? Frontmatter, string? Body) ParseFrontmatter(string content)
    {
        var match = FrontmatterRegex().Match(content);

        if (!match.Success)
        {
            return (null, null);
        }

        var yamlContent = match.Groups[1].Value;
        var body = match.Groups[2].Value;

        try
        {
            var frontmatter = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yamlContent);
            return (frontmatter, body);
        }
        catch
        {
            return (null, null);
        }
    }

    /// <summary>
    /// Safely get a string value from frontmatter dictionary.
    /// </summary>
    private static string? GetFrontmatterValue(Dictionary<string, object> frontmatter, string key)
    {
        if (frontmatter.TryGetValue(key, out var value))
        {
            return value?.ToString();
        }
        return null;
    }

    /// <summary>
    /// Safely get a list of strings from frontmatter dictionary.
    /// </summary>
    private static List<string> GetFrontmatterList(Dictionary<string, object> frontmatter, string key)
    {
        if (frontmatter.TryGetValue(key, out var value) && value is List<object> list)
        {
            return list.Select(item => item?.ToString() ?? string.Empty)
                       .Where(s => !string.IsNullOrEmpty(s))
                       .ToList();
        }
        return new List<string>();
    }

    /// <summary>
    /// Discover all resources in standard folders without loading content.
    /// </summary>
    private void DiscoverResources(SkillDefinition skill)
    {
        // Templates
        DiscoverResourcesInFolder(
            skill,
            skill.Templates,
            _config.TemplatesDirectory,
            _config.TemplatePattern,
            SkillResourceType.Template);

        // References
        DiscoverResourcesInFolder(
            skill,
            skill.References,
            _config.ReferencesDirectory,
            _config.ReferencePattern,
            SkillResourceType.Reference);

        // Scripts
        DiscoverResourcesInFolder(
            skill,
            skill.Scripts,
            _config.ScriptsDirectory,
            _config.ScriptPattern,
            SkillResourceType.Script);

        // Assets
        DiscoverResourcesInFolder(
            skill,
            skill.Assets,
            _config.AssetsDirectory,
            _config.AssetPattern,
            SkillResourceType.Asset);
    }

    /// <summary>
    /// Discover resources in a specific folder.
    /// </summary>
    private void DiscoverResourcesInFolder(
        SkillDefinition skill,
        List<SkillResource> resources,
        string folderName,
        string pattern,
        SkillResourceType resourceType)
    {
        var folderPath = Path.Combine(skill.BaseDirectory, folderName);

        if (!Directory.Exists(folderPath))
        {
            return;
        }

        // Use AllDirectories for assets to support nested folder structures (e.g., assets/brand/, assets/templates/)
        // Use TopDirectoryOnly for other resource types to avoid unintended discovery
        var searchOption = resourceType == SkillResourceType.Asset
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        var files = Directory.GetFiles(folderPath, pattern, searchOption);

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            var resource = new SkillResource
            {
                FileName = Path.GetFileName(file),
                FilePath = file,
                RelativePath = Path.GetRelativePath(skill.BaseDirectory, file),
                ResourceType = resourceType,
                IsLoaded = false,
                FileSize = fileInfo.Length,
                LastModified = fileInfo.LastWriteTimeUtc
            };
            resources.Add(resource);
        }
    }

    /// <summary>
    /// Load all resource contents (for eager loading mode).
    /// </summary>
    private async Task LoadAllResourceContentsAsync(SkillDefinition skill, CancellationToken ct)
    {
        foreach (var resource in skill.AllResources)
        {
            await LoadResourceContentAsync(resource, ct);
        }
    }
}
