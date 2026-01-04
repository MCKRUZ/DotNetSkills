using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SkillsQuickstart.Config;
using SkillsQuickstart.Services;

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
    Console.WriteLine($"│    Templates:  {skill.Templates.Count}");
    Console.WriteLine($"│    References: {skill.References.Count}");
    Console.WriteLine($"│    Scripts:    {skill.Scripts.Count}");
    Console.WriteLine($"│    Assets:     {skill.Assets.Count}");
    Console.WriteLine($"│  Instructions Loaded: {skill.Instructions != null}");
    Console.WriteLine($"└─ Fully Loaded: {skill.IsFullyLoaded}\n");
}

// ═══════════════════════════════════════════════════════════════════════════
// LEVEL 2: Full Skill Load (Instructions + Resource Inventory)
// Now we load the markdown body (instructions) for a specific skill
// ═══════════════════════════════════════════════════════════════════════════

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine(" LEVEL 2: Full Skill Load");
Console.WriteLine(" Loading 'code-review' with instructions and resource details.");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

var codeReview = await skillLoader.LoadSkillAsync("code-review");

if (codeReview != null)
{
    Console.WriteLine($"Loaded: {codeReview.Name}");
    Console.WriteLine($"Version: {codeReview.Version ?? "not specified"}");
    Console.WriteLine($"Is Fully Loaded: {codeReview.IsFullyLoaded}");
    Console.WriteLine($"Instructions Length: {codeReview.Instructions?.Length ?? 0} characters\n");

    // Show first 500 chars of instructions
    if (codeReview.Instructions != null)
    {
        var preview = codeReview.Instructions.Length > 500
            ? codeReview.Instructions[..500] + "..."
            : codeReview.Instructions;
        Console.WriteLine("Instructions Preview:");
        Console.WriteLine("─────────────────────");
        Console.WriteLine(preview);
        Console.WriteLine("─────────────────────\n");
    }

    // List discovered resources
    Console.WriteLine("Resource Inventory:");
    foreach (var resource in codeReview.AllResources)
    {
        var loadedStatus = resource.IsLoaded ? "[LOADED]" : "[pending]";
        Console.WriteLine($"  {resource.ResourceType,-10} {resource.RelativePath,-40} {loadedStatus}");
    }
    Console.WriteLine();
}
else
{
    Console.WriteLine("Skill 'code-review' not found.\n");
}

// ═══════════════════════════════════════════════════════════════════════════
// LEVEL 3: Resource Loading (On-Demand Content)
// Load actual file content only when needed
// ═══════════════════════════════════════════════════════════════════════════

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine(" LEVEL 3: Resource Loading (On-Demand)");
Console.WriteLine(" Loading template content only when explicitly requested.");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

if (codeReview?.Templates.FirstOrDefault() is { } template)
{
    Console.WriteLine($"Loading: {template.RelativePath}");
    Console.WriteLine($"File Size: {template.FileSize} bytes");
    Console.WriteLine($"Was Loaded: {template.IsLoaded}\n");

    var content = await skillLoader.LoadResourceContentAsync(template);

    Console.WriteLine($"Now Loaded: {template.IsLoaded}");
    Console.WriteLine($"Content Length: {content?.Length ?? 0} characters\n");

    if (content != null)
    {
        Console.WriteLine("Template Content:");
        Console.WriteLine("─────────────────────");
        Console.WriteLine(content);
        Console.WriteLine("─────────────────────\n");
    }
}
else
{
    Console.WriteLine("No templates found for code-review skill.\n");
}

// Also load a reference file to show multiple resource types
if (codeReview?.References.FirstOrDefault() is { } reference)
{
    Console.WriteLine($"Loading Reference: {reference.RelativePath}");
    var refContent = await skillLoader.LoadResourceContentAsync(reference);

    if (refContent != null)
    {
        var preview = refContent.Length > 400
            ? refContent[..400] + "..."
            : refContent;
        Console.WriteLine("Reference Content Preview:");
        Console.WriteLine("─────────────────────");
        Console.WriteLine(preview);
        Console.WriteLine("─────────────────────\n");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// BONUS: Simulated Agent Context
// Shows how a skill would be formatted for injection into an AI prompt
// ═══════════════════════════════════════════════════════════════════════════

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine(" BONUS: Simulated Agent Context");
Console.WriteLine(" How this skill would appear in an agent's system prompt.");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

if (codeReview != null)
{
    var templateList = codeReview.HasTemplates
        ? string.Join("\n", codeReview.Templates.Select(t => $"  - {t.RelativePath}"))
        : "  (none)";

    var referenceList = codeReview.HasReferences
        ? string.Join("\n", codeReview.References.Select(r => $"  - {r.RelativePath}"))
        : "  (none)";

    var agentContext = $"""
        ┌──────────────────────────────────────────────────────────────┐
        │ SKILL: {codeReview.Name,-52} │
        └──────────────────────────────────────────────────────────────┘

        {codeReview.Instructions}

        ══════════════════════════════════════════════════════════════
        Available Resources:
        ══════════════════════════════════════════════════════════════

        Templates:
        {templateList}

        References:
        {referenceList}

        Note: Request resource content using LoadResourceContentAsync()
        when you need to use templates or consult reference material.
        """;

    Console.WriteLine(agentContext);
}

// ═══════════════════════════════════════════════════════════════════════════
// Summary
// ═══════════════════════════════════════════════════════════════════════════

Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
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
