using SkillsQuickstart.Models;

namespace SkillsQuickstart.Services;

/// <summary>
/// Service for discovering and loading skills following the progressive disclosure pattern.
/// </summary>
/// <remarks>
/// Progressive disclosure levels:
/// 1. DiscoverSkillsAsync - Returns metadata only (name, description, resource counts)
/// 2. LoadSkillAsync - Returns full skill with instructions and resource inventory
/// 3. LoadResourceContentAsync - Returns actual content of a specific resource
/// </remarks>
public interface ISkillLoader
{
    /// <summary>
    /// Discover all skills, loading only metadata (name, description).
    /// This is the first level of progressive disclosure.
    /// </summary>
    /// <remarks>
    /// Use this for displaying skill lists or catalogs.
    /// Instructions are NOT loaded at this level.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of skill definitions with metadata only.</returns>
    Task<IReadOnlyList<SkillDefinition>> DiscoverSkillsAsync(CancellationToken ct = default);

    /// <summary>
    /// Load a skill fully, including instructions and resource inventory.
    /// This is the second level of progressive disclosure.
    /// </summary>
    /// <remarks>
    /// Resource file metadata is populated, but content is NOT loaded.
    /// The skill is marked as IsFullyLoaded = true.
    /// </remarks>
    /// <param name="skillId">The skill ID (folder name).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Fully loaded skill definition, or null if not found.</returns>
    Task<SkillDefinition?> LoadSkillAsync(string skillId, CancellationToken ct = default);

    /// <summary>
    /// Load a specific resource's content on demand.
    /// This is the third level of progressive disclosure.
    /// </summary>
    /// <remarks>
    /// After loading, resource.Content and resource.IsLoaded are updated.
    /// Subsequent calls return the cached content.
    /// </remarks>
    /// <param name="resource">The resource to load content for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resource content, or null if file doesn't exist.</returns>
    Task<string?> LoadResourceContentAsync(SkillResource resource, CancellationToken ct = default);

    /// <summary>
    /// Get skill by ID without full loading (from cache or discovery).
    /// </summary>
    /// <remarks>
    /// Returns metadata-only version if skill hasn't been fully loaded.
    /// Use LoadSkillAsync if you need instructions.
    /// </remarks>
    /// <param name="skillId">The skill ID (folder name).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Skill definition (possibly metadata-only), or null if not found.</returns>
    Task<SkillDefinition?> GetSkillMetadataAsync(string skillId, CancellationToken ct = default);

    /// <summary>
    /// Search for skills by tag.
    /// </summary>
    /// <param name="tag">Tag to search for (case-insensitive).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Skills that have the specified tag.</returns>
    Task<IReadOnlyList<SkillDefinition>> FindSkillsByTagAsync(string tag, CancellationToken ct = default);

    /// <summary>
    /// Invalidate the cache and force rediscovery on next access.
    /// </summary>
    void InvalidateCache();
}
