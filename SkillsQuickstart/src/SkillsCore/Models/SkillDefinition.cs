namespace SkillsCore.Models;

/// <summary>
/// Represents a skill loaded from a SKILL.md file.
/// Mirrors Anthropic's Agent Skills format used in Claude Code.
/// </summary>
/// <remarks>
/// Skills follow a progressive disclosure pattern:
/// 1. Discovery: Only name/description loaded (for listing)
/// 2. Full Load: Instructions and resource inventory loaded
/// 3. Resource Load: Individual resource contents loaded on demand
/// </remarks>
public class SkillDefinition
{
    // ═══════════════════════════════════════════════════════════════════
    // Core Properties (Claude Code standard frontmatter fields)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Unique identifier derived from the skill folder name.
    /// Used for lookups and caching (e.g., "code-review").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name from frontmatter 'name' field.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brief description from frontmatter 'description' field.
    /// Should explain when to use this skill.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Full markdown content after the frontmatter.
    /// Contains detailed instructions for executing the skill.
    /// Only populated after full skill load (Level 2).
    /// </summary>
    public string? Instructions { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // Extended Metadata (optional frontmatter fields)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Semantic version of the skill (e.g., "1.0.0").
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Skill author or maintainer.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Primary category for organization (e.g., "development", "documentation").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Tags for filtering and discovery.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    // ═══════════════════════════════════════════════════════════════════
    // File System Location
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Absolute path to the SKILL.md file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Absolute path to the skill's root directory.
    /// Resources are discovered relative to this path.
    /// </summary>
    public string BaseDirectory { get; set; } = string.Empty;

    /// <summary>
    /// When this skill definition was loaded into memory.
    /// </summary>
    public DateTime LoadedAt { get; set; }

    /// <summary>
    /// Last modification time of the SKILL.md file.
    /// Used for cache invalidation.
    /// </summary>
    public DateTime LastModified { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // Resources (Claude Code standard folders)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Output templates from the templates/ folder.
    /// </summary>
    public List<SkillResource> Templates { get; set; } = new();

    /// <summary>
    /// Reference documentation from the references/ folder.
    /// </summary>
    public List<SkillResource> References { get; set; } = new();

    /// <summary>
    /// Executable scripts from the scripts/ folder.
    /// </summary>
    public List<SkillResource> Scripts { get; set; } = new();

    /// <summary>
    /// Binary/static assets from the assets/ folder.
    /// </summary>
    public List<SkillResource> Assets { get; set; } = new();

    // ═══════════════════════════════════════════════════════════════════
    // Loading State
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// True when instructions and resource inventory have been loaded (Level 2).
    /// </summary>
    public bool IsFullyLoaded { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // Extensibility
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Additional frontmatter fields not mapped to specific properties.
    /// Allows skills to define custom metadata without code changes.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    // ═══════════════════════════════════════════════════════════════════
    // Computed Properties
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether this skill has any templates.
    /// </summary>
    public bool HasTemplates => Templates.Count > 0;

    /// <summary>
    /// Whether this skill has any reference documentation.
    /// </summary>
    public bool HasReferences => References.Count > 0;

    /// <summary>
    /// Whether this skill has any scripts.
    /// </summary>
    public bool HasScripts => Scripts.Count > 0;

    /// <summary>
    /// Whether this skill has any assets.
    /// </summary>
    public bool HasAssets => Assets.Count > 0;

    /// <summary>
    /// Total count of all resources across all categories.
    /// </summary>
    public int TotalResourceCount => Templates.Count + References.Count + Scripts.Count + Assets.Count;

    /// <summary>
    /// All resources flattened into a single enumerable.
    /// </summary>
    public IEnumerable<SkillResource> AllResources =>
        Templates.Concat(References).Concat(Scripts).Concat(Assets);

    public override string ToString() => $"{Name} ({Id}) - {TotalResourceCount} resources";
}
