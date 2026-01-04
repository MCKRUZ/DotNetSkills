namespace SkillsQuickstart.Config;

/// <summary>
/// Configuration for the skills loader system.
/// Bound from appsettings.json "Skills" section.
/// </summary>
/// <remarks>
/// Default values match Claude Code conventions for maximum compatibility.
/// </remarks>
public class SkillsConfig
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Skills";

    // ═══════════════════════════════════════════════════════════════════
    // Path Configuration
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Base path where skills are located, relative to the application root.
    /// Default: "skills"
    /// </summary>
    public string BasePath { get; set; } = "skills";

    /// <summary>
    /// Name of the skill definition file to look for in each skill folder.
    /// Default: "SKILL.md" (matches Claude Code convention)
    /// </summary>
    public string SkillFileName { get; set; } = "SKILL.md";

    // ═══════════════════════════════════════════════════════════════════
    // Standard Folder Names (Claude Code convention)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Folder name for output templates.
    /// Default: "templates"
    /// </summary>
    public string TemplatesDirectory { get; set; } = "templates";

    /// <summary>
    /// Folder name for reference documentation.
    /// Default: "references"
    /// </summary>
    public string ReferencesDirectory { get; set; } = "references";

    /// <summary>
    /// Folder name for executable scripts.
    /// Default: "scripts"
    /// </summary>
    public string ScriptsDirectory { get; set; } = "scripts";

    /// <summary>
    /// Folder name for binary/static assets.
    /// Default: "assets"
    /// </summary>
    public string AssetsDirectory { get; set; } = "assets";

    // ═══════════════════════════════════════════════════════════════════
    // File Patterns
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Glob pattern for discovering template files.
    /// Default: "*.template.*" (e.g., report.template.md)
    /// </summary>
    public string TemplatePattern { get; set; } = "*.template.*";

    /// <summary>
    /// Glob pattern for discovering reference files.
    /// Default: "*.md"
    /// </summary>
    public string ReferencePattern { get; set; } = "*.md";

    /// <summary>
    /// Glob pattern for discovering script files.
    /// Default: "*.*" (all files)
    /// </summary>
    public string ScriptPattern { get; set; } = "*.*";

    /// <summary>
    /// Glob pattern for discovering asset files.
    /// Default: "*.*" (all files)
    /// </summary>
    public string AssetPattern { get; set; } = "*.*";

    // ═══════════════════════════════════════════════════════════════════
    // Caching Configuration
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// How long to cache skill definitions before checking for changes.
    /// Default: 5 minutes
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 5;

    /// <summary>
    /// If true, load all resource contents during skill discovery.
    /// If false (default), resources are loaded on-demand.
    /// </summary>
    public bool EagerLoadResources { get; set; } = false;

    // ═══════════════════════════════════════════════════════════════════
    // Computed Properties
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cache duration as a TimeSpan.
    /// </summary>
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(CacheDurationMinutes);
}
