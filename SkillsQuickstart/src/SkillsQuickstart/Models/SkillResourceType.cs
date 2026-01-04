namespace SkillsQuickstart.Models;

/// <summary>
/// Categorizes skill resources by their intended purpose.
/// Maps directly to the standard Claude Code folder structure.
/// </summary>
public enum SkillResourceType
{
    /// <summary>
    /// Output templates with placeholders (templates/ folder).
    /// Used for generating structured output like reports, code files, etc.
    /// </summary>
    Template,

    /// <summary>
    /// Reference documentation (references/ folder).
    /// Guidelines, standards, and background information for the skill.
    /// </summary>
    Reference,

    /// <summary>
    /// Executable scripts (scripts/ folder).
    /// Automation scripts that can be invoked by the skill.
    /// </summary>
    Script,

    /// <summary>
    /// Binary or static assets (assets/ folder).
    /// Images, schemas, data files, and other non-text resources.
    /// </summary>
    Asset
}
