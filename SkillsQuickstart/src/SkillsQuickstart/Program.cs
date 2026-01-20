using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SkillsCore.Config;
using SkillsCore.Services;

// ═══════════════════════════════════════════════════════════════════════════
// Skills Loader Quickstart
// Demonstrates Anthropic's Agent Skills pattern with progressive disclosure
// ═══════════════════════════════════════════════════════════════════════════

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║       Agent Skills Loader - Progressive Disclosure Demo      ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

// ─────────────────────────────────────────────────────────────────────────────
// Setup: Configuration and Dependency Injection
// ─────────────────────────────────────────────────────────────────────────────

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();

// Bind configuration using IOptions pattern
services.Configure<SkillsConfig>(configuration.GetSection(SkillsConfig.SectionName));

// Register skill loader service
services.AddSingleton<ISkillLoader, SkillLoaderService>();

var serviceProvider = services.BuildServiceProvider();
var skillLoader = serviceProvider.GetRequiredService<ISkillLoader>();

// Display configuration
var config = serviceProvider.GetRequiredService<IOptions<SkillsConfig>>().Value;
Console.WriteLine($"Skills Base Path: {Path.Combine(AppContext.BaseDirectory, config.BasePath)}");
Console.WriteLine($"Skill File Name:  {config.SkillFileName}");
Console.WriteLine($"Cache Duration:   {config.CacheDurationMinutes} minutes\n");

// ═══════════════════════════════════════════════════════════════════════════
// LEVEL 1: Discovery (Metadata Only)
// This is the lightest load - just names, descriptions, and resource counts
// ═══════════════════════════════════════════════════════════════════════════

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine(" LEVEL 1: Discovery (Metadata Only)");
Console.WriteLine(" Instructions are NOT loaded at this level.");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

var skills = await skillLoader.DiscoverSkillsAsync();

if (skills.Count == 0)
{
    Console.WriteLine("No skills found. Ensure the 'skills' folder exists with SKILL.md files.\n");
    Console.WriteLine("Expected structure:");
    Console.WriteLine("  skills/");
    Console.WriteLine("    └── your-skill/");
    Console.WriteLine("        └── SKILL.md\n");
    return;
}

foreach (var skill in skills)
{
    Console.WriteLine($"┌─ {skill.Name} ({skill.Id})");
    Console.WriteLine($"│  {skill.Description}");
    Console.WriteLine($"│  Tags: [{string.Join(", ", skill.Tags)}]");
    Console.WriteLine($"│  Resources: {skill.TotalResourceCount} files");
    Console.WriteLine($"│  Instructions Loaded: {skill.Instructions != null}");
    Console.WriteLine($"└─ Fully Loaded: {skill.IsFullyLoaded}\n");
}

// ═══════════════════════════════════════════════════════════════════════════
// LEVEL 2: Full Skill Load (Instructions + Resource Inventory)
// Now we load the markdown body (instructions) for the first available skill
// ═══════════════════════════════════════════════════════════════════════════

var firstSkill = skills.FirstOrDefault();

if (firstSkill == null)
{
    Console.WriteLine("No skills available to load.\n");
    return;
}

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine(" LEVEL 2: Full Skill Load");
Console.WriteLine($" Loading '{firstSkill.Name}' with instructions.");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

var loadedSkill = await skillLoader.LoadSkillAsync(firstSkill.Id);

if (loadedSkill != null)
{
    Console.WriteLine($"Loaded: {loadedSkill.Name}");
    Console.WriteLine($"Version: {loadedSkill.Version ?? "not specified"}");
    Console.WriteLine($"Is Fully Loaded: {loadedSkill.IsFullyLoaded}");
    Console.WriteLine($"Instructions Length: {loadedSkill.Instructions?.Length ?? 0} characters\n");

    // Show first 500 chars of instructions
    if (loadedSkill.Instructions != null)
    {
        var preview = loadedSkill.Instructions.Length > 500
            ? loadedSkill.Instructions[..500] + "..."
            : loadedSkill.Instructions;
        Console.WriteLine("Instructions Preview:");
        Console.WriteLine("─────────────────────");
        Console.WriteLine(preview);
        Console.WriteLine("─────────────────────\n");
    }

    // List discovered resources
    Console.WriteLine("Resource Inventory:");
    foreach (var resource in loadedSkill.AllResources)
    {
        var loadedStatus = resource.IsLoaded ? "[LOADED]" : "[pending]";
        Console.WriteLine($"  {resource.ResourceType,-10} {resource.RelativePath,-40} {loadedStatus}");
    }
    Console.WriteLine();
}

// ═══════════════════════════════════════════════════════════════════════════
// LEVEL 3: Resource Loading (On-Demand Content)
// Load actual file content only when needed
// ═══════════════════════════════════════════════════════════════════════════

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine(" LEVEL 3: Resource Loading (On-Demand)");
Console.WriteLine(" Loading template content only when explicitly requested.");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

if (loadedSkill?.Templates.FirstOrDefault() is { } template)
{
    Console.WriteLine($"Loading: {template.RelativePath}");
    Console.WriteLine($"File Size: {template.FileSize} bytes");
    Console.WriteLine($"Was Loaded: {template.IsLoaded}\n");

    var content = await skillLoader.LoadResourceContentAsync(template);

    Console.WriteLine($"Now Loaded: {template.IsLoaded}");
    Console.WriteLine($"Content Length: {content?.Length ?? 0} characters\n");

    if (content != null)
    {
        var preview = content.Length > 500 ? content[..500] + "..." : content;
        Console.WriteLine("Template Content:");
        Console.WriteLine("─────────────────────");
        Console.WriteLine(preview);
        Console.WriteLine("─────────────────────\n");
    }
}
else
{
    Console.WriteLine($"No templates found for {firstSkill.Name} skill.\n");
}

// ═══════════════════════════════════════════════════════════════════════════
// Summary
// ═══════════════════════════════════════════════════════════════════════════

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine(" Summary: Progressive Disclosure Levels");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine(" Level 1 │ DiscoverSkillsAsync()      │ Metadata only (fast)");
Console.WriteLine(" Level 2 │ LoadSkillAsync()           │ + Instructions loaded");
Console.WriteLine(" Level 3 │ LoadResourceContentAsync() │ + Resource content loaded");
Console.WriteLine();
Console.WriteLine("This pattern minimizes memory usage and load times by");
Console.WriteLine("deferring expensive operations until actually needed.");
Console.WriteLine();
Console.WriteLine("To execute skills with an AI model, use the SkillsMcpServer project");
Console.WriteLine("which exposes skills as MCP tools, resources, and prompts.");
Console.WriteLine();
