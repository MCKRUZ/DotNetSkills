using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SkillsQuickstart.Config;
using SkillsQuickstart.Models;
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
services.Configure<AzureOpenAIConfig>(configuration.GetSection(AzureOpenAIConfig.SectionName));

// Register skill loader service
services.AddSingleton<ISkillLoader, SkillLoaderService>();

// Register skill executor service
services.AddSingleton<ISkillExecutor, AzureOpenAISkillExecutor>();

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
Console.WriteLine($" Loading '{firstSkill.Name}' ({firstSkill.Id}) with instructions and resource details.");
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
else
{
    Console.WriteLine($"Skill '{firstSkill.Id}' not found.\n");
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
        Console.WriteLine("Template Content:");
        Console.WriteLine("─────────────────────");
        Console.WriteLine(content);
        Console.WriteLine("─────────────────────\n");
    }
}
else
{
    Console.WriteLine($"No templates found for {firstSkill.Name} skill.\n");
}

// Also load a reference file to show multiple resource types
if (loadedSkill?.References.FirstOrDefault() is { } reference)
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

if (loadedSkill != null)
{
    var templateList = loadedSkill.HasTemplates
        ? string.Join("\n", loadedSkill.Templates.Select(t => $"  - {t.RelativePath}"))
        : "  (none)";

    var referenceList = loadedSkill.HasReferences
        ? string.Join("\n", loadedSkill.References.Select(r => $"  - {r.RelativePath}"))
        : "  (none)";

    var agentContext = $"""
        ┌──────────────────────────────────────────────────────────────┐
        │ SKILL: {loadedSkill.Name,-52} │
        └──────────────────────────────────────────────────────────────┘

        {loadedSkill.Instructions}

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

// ═══════════════════════════════════════════════════════════════════════════
// BONUS: Execute Skill with Azure OpenAI
// Demonstrates running the loaded skill with actual AI execution
// ═══════════════════════════════════════════════════════════════════════════

var skillExecutor = serviceProvider.GetRequiredService<ISkillExecutor>();

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine(" BONUS: Execute Skill with Azure OpenAI");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

if (skillExecutor.IsConfigured)
{
    var azureConfig = serviceProvider.GetRequiredService<IOptions<AzureOpenAIConfig>>().Value;
    Console.WriteLine($"Azure OpenAI Configuration:");
    Console.WriteLine($"  Endpoint:        {azureConfig.Endpoint}");
    Console.WriteLine($"  Deployment:      {azureConfig.DeploymentName}");
    Console.WriteLine($"  Model:           {azureConfig.ModelName}");
    Console.WriteLine($"  Max Tokens:      {azureConfig.MaxTokens}");
    Console.WriteLine($"  Temperature:     {azureConfig.Temperature}");
    Console.WriteLine();

    if (loadedSkill != null)
    {
        Console.WriteLine($"Executing '{loadedSkill.Name}' skill...");
        Console.WriteLine($"(In a real application, you would provide user input here)");
        Console.WriteLine();

        // Example user input - in a real app this would come from the user
        var exampleInput = "Hello! Please help me with a task.";
        Console.WriteLine($"Example Input: {exampleInput}");
        Console.WriteLine();
        Console.WriteLine("─".PadRight(60, '─'));
        Console.WriteLine("Note: Azure OpenAI execution is ready. To execute the skill,");
        Console.WriteLine("provide actual user input and uncomment the execution below.");
        Console.WriteLine("─".PadRight(60, '─'));
        Console.WriteLine();

        // Uncomment to actually execute the skill with Azure OpenAI:
        /*
        try
        {
            var response = await skillExecutor.ExecuteAsync(loadedSkill, exampleInput);
            Console.WriteLine("\nAI Response:");
            Console.WriteLine("─────────────────────");
            Console.WriteLine(response);
            Console.WriteLine("─────────────────────\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}\n");
        }
        */
    }
}
else
{
    Console.WriteLine("Azure OpenAI is not configured.");
    Console.WriteLine();
    Console.WriteLine("To enable skill execution, update appsettings.json:");
    Console.WriteLine("  {");
    Console.WriteLine("    \"AzureOpenAI\": {");
    Console.WriteLine("      \"Endpoint\": \"https://your-resource.openai.azure.com/\",");
    Console.WriteLine("      \"ApiKey\": \"your-api-key\",");
    Console.WriteLine("      \"DeploymentName\": \"your-deployment-name\"");
    Console.WriteLine("    }");
    Console.WriteLine("  }");
    Console.WriteLine();
}
