using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;
using SkillsCore.Services;

namespace SkillsMcpServer.Tools;

/// <summary>
/// MCP tools for discovering and working with skills.
/// These tools allow Claude to explore available skills dynamically.
/// </summary>
[McpServerToolType]
public class SkillTools
{
    private readonly ISkillLoader _skillLoader;

    public SkillTools(ISkillLoader skillLoader)
    {
        _skillLoader = skillLoader;
    }

    /// <summary>
    /// Lists all available skills with their metadata.
    /// </summary>
    [McpServerTool(Name = "list_skills"), Description("Lists all available skills with their names, descriptions, and resource counts. Use this to discover what skills are available.")]
    public async Task<string> ListSkillsAsync(
        [Description("Optional tag to filter skills by")] string? tag = null,
        CancellationToken cancellationToken = default)
    {
        var skills = string.IsNullOrEmpty(tag)
            ? await _skillLoader.DiscoverSkillsAsync(cancellationToken)
            : await _skillLoader.FindSkillsByTagAsync(tag, cancellationToken);

        if (skills.Count == 0)
        {
            return string.IsNullOrEmpty(tag)
                ? "No skills found. Ensure the skills folder contains SKILL.md files."
                : $"No skills found with tag '{tag}'.";
        }

        var output = new StringBuilder();
        output.AppendLine($"# Available Skills ({skills.Count})");
        output.AppendLine();

        foreach (var skill in skills)
        {
            output.AppendLine($"## {skill.Name} (`{skill.Id}`)");
            output.AppendLine();
            output.AppendLine(skill.Description);
            output.AppendLine();

            if (skill.Tags.Count > 0)
            {
                output.AppendLine($"**Tags:** {string.Join(", ", skill.Tags)}");
            }

            output.AppendLine($"**Resources:** {skill.TotalResourceCount} files");
            output.AppendLine($"  - Templates: {skill.Templates.Count}");
            output.AppendLine($"  - References: {skill.References.Count}");
            output.AppendLine($"  - Scripts: {skill.Scripts.Count}");
            output.AppendLine($"  - Assets: {skill.Assets.Count}");
            output.AppendLine();
        }

        output.AppendLine("---");
        output.AppendLine("Use `get_skill_details` to load full instructions for a specific skill.");

        return output.ToString();
    }

    /// <summary>
    /// Gets detailed information about a specific skill including its instructions.
    /// </summary>
    [McpServerTool(Name = "get_skill_details"), Description("Gets full details about a skill including its instructions and available resources. Use this to understand what a skill does before using it.")]
    public async Task<string> GetSkillDetailsAsync(
        [Description("The skill ID (folder name) to get details for")] string skillId,
        CancellationToken cancellationToken = default)
    {
        var skill = await _skillLoader.LoadSkillAsync(skillId, cancellationToken);

        if (skill == null)
        {
            return $"Skill '{skillId}' not found. Use `list_skills` to see available skills.";
        }

        var output = new StringBuilder();
        output.AppendLine($"# {skill.Name}");
        output.AppendLine();
        output.AppendLine($"**ID:** {skill.Id}");
        output.AppendLine($"**Version:** {skill.Version ?? "not specified"}");
        output.AppendLine($"**Author:** {skill.Author ?? "not specified"}");
        output.AppendLine($"**Category:** {skill.Category ?? "not specified"}");

        if (skill.Tags.Count > 0)
        {
            output.AppendLine($"**Tags:** {string.Join(", ", skill.Tags)}");
        }

        output.AppendLine();
        output.AppendLine("## Description");
        output.AppendLine();
        output.AppendLine(skill.Description);
        output.AppendLine();

        output.AppendLine("## Instructions");
        output.AppendLine();
        output.AppendLine(skill.Instructions ?? "No instructions available.");
        output.AppendLine();

        if (skill.TotalResourceCount > 0)
        {
            output.AppendLine("## Available Resources");
            output.AppendLine();

            if (skill.HasTemplates)
            {
                output.AppendLine("### Templates");
                foreach (var template in skill.Templates)
                {
                    output.AppendLine($"- `{template.RelativePath}` ({template.FileSize} bytes)");
                }
                output.AppendLine();
            }

            if (skill.HasReferences)
            {
                output.AppendLine("### References");
                foreach (var reference in skill.References)
                {
                    output.AppendLine($"- `{reference.RelativePath}` ({reference.FileSize} bytes)");
                }
                output.AppendLine();
            }

            if (skill.HasScripts)
            {
                output.AppendLine("### Scripts");
                foreach (var script in skill.Scripts)
                {
                    output.AppendLine($"- `{script.RelativePath}` ({script.FileSize} bytes)");
                }
                output.AppendLine();
            }

            if (skill.HasAssets)
            {
                output.AppendLine("### Assets");
                foreach (var asset in skill.Assets)
                {
                    output.AppendLine($"- `{asset.RelativePath}` ({asset.FileSize} bytes)");
                }
                output.AppendLine();
            }

            output.AppendLine("---");
            output.AppendLine($"Use `read_skill_resource` to read resource contents.");
        }

        return output.ToString();
    }

    /// <summary>
    /// Reads the content of a skill resource.
    /// </summary>
    [McpServerTool(Name = "read_skill_resource"), Description("Reads the content of a specific resource file from a skill. Use this to access templates, references, or other bundled files.")]
    public async Task<string> ReadSkillResourceAsync(
        [Description("The skill ID containing the resource")] string skillId,
        [Description("The relative path to the resource (e.g., 'templates/report.template.md')")] string resourcePath,
        CancellationToken cancellationToken = default)
    {
        var skill = await _skillLoader.LoadSkillAsync(skillId, cancellationToken);

        if (skill == null)
        {
            return $"Skill '{skillId}' not found.";
        }

        // Normalize path separators
        var normalizedPath = resourcePath.Replace('\\', '/');

        var resource = skill.AllResources.FirstOrDefault(r =>
            r.RelativePath.Replace('\\', '/').Equals(normalizedPath, StringComparison.OrdinalIgnoreCase));

        if (resource == null)
        {
            var availableResources = string.Join("\n", skill.AllResources.Select(r => $"  - {r.RelativePath}"));
            return $"Resource '{resourcePath}' not found in skill '{skillId}'.\n\nAvailable resources:\n{availableResources}";
        }

        var content = await _skillLoader.LoadResourceContentAsync(resource, cancellationToken);

        if (content == null)
        {
            return $"Could not read content of resource '{resourcePath}'.";
        }

        return $"# {resource.FileName}\n\n```\n{content}\n```";
    }
}
