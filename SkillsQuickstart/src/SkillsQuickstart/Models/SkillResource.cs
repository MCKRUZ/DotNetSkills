namespace SkillsQuickstart.Models;

/// <summary>
/// Represents a file bundled with a skill.
/// Resources are discovered but not loaded until needed (progressive disclosure).
/// </summary>
/// <remarks>
/// This implements the "lazy loading" pattern where file metadata is captured
/// during discovery, but actual content is only read when explicitly requested.
/// This keeps initial skill loading fast and memory-efficient.
/// </remarks>
public class SkillResource
{
    /// <summary>
    /// The file name without path (e.g., "review-report.template.md").
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Absolute path to the resource file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Path relative to the skill's base directory (e.g., "templates/review-report.template.md").
    /// Useful for display and referencing in skill instructions.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// The category of this resource based on its containing folder.
    /// </summary>
    public SkillResourceType ResourceType { get; set; }

    /// <summary>
    /// The file content, populated only after explicit loading.
    /// Null until LoadResourceContentAsync is called.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Indicates whether the Content property has been populated.
    /// </summary>
    public bool IsLoaded { get; set; }

    /// <summary>
    /// File size in bytes, captured during discovery for estimation purposes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Last modification time of the resource file.
    /// </summary>
    public DateTime LastModified { get; set; }

    public override string ToString() => $"{ResourceType}: {RelativePath}";
}
