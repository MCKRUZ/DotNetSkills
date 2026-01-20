# Running Anthropic Skills in .NET with MCP Integration

A complete implementation of Anthropic's Agent Skills pattern in .NET, featuring progressive disclosure for memory efficiency and Model Context Protocol (MCP) integration for seamless Claude connectivity.

## Table of Contents

1. [Introduction](#introduction)
2. [What Are Anthropic Skills?](#what-are-anthropic-skills)
3. [The Problem This Solves](#the-problem-this-solves)
4. [Architecture Overview](#architecture-overview)
5. [Project Structure](#project-structure)
6. [Core Concepts](#core-concepts)
   - [Progressive Disclosure Pattern](#progressive-disclosure-pattern)
   - [SKILL.md Format](#skillmd-format)
   - [Resource Management](#resource-management)
7. [Implementation Deep Dive](#implementation-deep-dive)
   - [SkillsCore Library](#skillscore-library)
   - [Skill Loading Service](#skill-loading-service)
   - [Configuration System](#configuration-system)
8. [MCP Server Integration](#mcp-server-integration)
   - [What is MCP?](#what-is-mcp)
   - [MCP Tools Implementation](#mcp-tools-implementation)
   - [Connecting to Claude Desktop](#connecting-to-claude-desktop)
9. [Building Your Own Skills](#building-your-own-skills)
10. [Running the Solution](#running-the-solution)
11. [Real-World Example: Executive Deck Generator](#real-world-example-executive-deck-generator)

---

## Introduction

This project demonstrates how to implement Anthropic's Agent Skills pattern in a .NET environment. Skills are reusable, self-contained instruction sets that can be loaded dynamically and executed by AI agents. Think of them as "plugins" for AI assistants - each skill contains instructions, templates, and reference materials that teach the AI how to perform a specific task.

The implementation includes:
- A core library (`SkillsCore`) for loading and managing skills
- A demo console application showing the progressive disclosure pattern
- An MCP server that exposes skills as tools Claude can use directly

---

## What Are Anthropic Skills?

Anthropic Skills are a standardized way to package AI capabilities. Each skill is defined in a `SKILL.md` file with:

1. **YAML Frontmatter** - Metadata (name, description, version, tags)
2. **Markdown Body** - Detailed instructions for the AI
3. **Resource Folders** - Supporting files organized by type:
   - `templates/` - Output templates (e.g., report formats)
   - `references/` - Background documentation
   - `scripts/` - Executable automation
   - `assets/` - Static files (images, schemas, etc.)

This format originated in Claude Code and provides a consistent way to extend AI capabilities without modifying the underlying system.

---

## The Problem This Solves

When building AI-powered applications, you face several challenges:

| Challenge | How Skills Solve It |
|-----------|---------------------|
| **Context window limits** | Progressive disclosure loads only what's needed |
| **Inconsistent AI behavior** | Standardized instructions ensure repeatable results |
| **Code duplication** | Skills are reusable across projects |
| **Maintenance overhead** | Skills are self-contained and versioned |
| **Integration complexity** | MCP provides standard protocol for tool access |

Without skills, you end up with:
- Massive system prompts that waste tokens
- Instructions scattered across your codebase
- No clear way to add new capabilities
- Tight coupling between AI logic and application code

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Your Application                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────┐    ┌──────────────────┐                   │
│  │  SkillsQuickstart │    │  SkillsMcpServer │                   │
│  │  (Demo Console)   │    │  (MCP Server)    │                   │
│  └────────┬─────────┘    └────────┬─────────┘                   │
│           │                       │                              │
│           └───────────┬───────────┘                              │
│                       │                                          │
│              ┌────────▼────────┐                                 │
│              │   SkillsCore    │                                 │
│              │   (Library)     │                                 │
│              └────────┬────────┘                                 │
│                       │                                          │
├───────────────────────┼──────────────────────────────────────────┤
│                       │                                          │
│              ┌────────▼────────┐                                 │
│              │     skills/     │                                 │
│              │   SKILL.md      │                                 │
│              │   templates/    │                                 │
│              │   references/   │                                 │
│              └─────────────────┘                                 │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ MCP Protocol (stdio)
                              ▼
                    ┌─────────────────┐
                    │  Claude Desktop │
                    │  or Claude Code │
                    └─────────────────┘
```

---

## Project Structure

```
DotNetSkills/
├── README.md                           # This file
└── SkillsQuickstart/
    ├── SkillsQuickstart.sln            # Solution file
    └── src/
        ├── SkillsCore/                 # Shared library
        │   ├── SkillsCore.csproj
        │   ├── Config/
        │   │   └── SkillsConfig.cs     # Configuration options
        │   ├── Models/
        │   │   ├── SkillDefinition.cs  # Skill data model
        │   │   ├── SkillResource.cs    # Resource data model
        │   │   └── SkillResourceType.cs # Resource type enum
        │   └── Services/
        │       ├── ISkillLoader.cs     # Loader interface
        │       └── SkillLoaderService.cs # Implementation
        │
        ├── SkillsQuickstart/           # Demo console app
        │   ├── SkillsQuickstart.csproj
        │   ├── Program.cs              # Progressive disclosure demo
        │   ├── appsettings.json        # Configuration
        │   └── skills/                 # Example skills
        │       ├── api-client/
        │       ├── code-review/
        │       ├── documentation/
        │       └── ey-executive-deck/
        │
        └── SkillsMcpServer/            # MCP Server
            ├── SkillsMcpServer.csproj
            ├── Program.cs              # Server entry point
            ├── appsettings.json
            └── Tools/
                ├── PresentationTools.cs # Deck generation tools
                └── SkillTools.cs        # Skill discovery tools
```

---

## Core Concepts

### Progressive Disclosure Pattern

The key innovation in this implementation is **progressive disclosure** - a three-level loading strategy that minimizes memory usage and improves performance:

```
Level 1: Discovery          Level 2: Full Load         Level 3: Resource Load
─────────────────────      ─────────────────────      ─────────────────────
• Name                     • Name                     • Name
• Description              • Description              • Description
• Tags                     • Tags                     • Tags
• Resource counts          • Resource counts          • Resource counts
                           • Full instructions        • Full instructions
                           • Resource metadata        • Resource metadata
                                                      • Resource CONTENT

~100 tokens                ~5,000 tokens              Unlimited
```

**Why this matters:**
- Level 1 is fast - scan 100 skills instantly to find the right one
- Level 2 loads only when you've selected a skill
- Level 3 loads resources only when the AI actually needs them

```csharp
// Level 1: Discover all skills (metadata only)
var skills = await skillLoader.DiscoverSkillsAsync();
// Result: Quick list with names and descriptions

// Level 2: Load a specific skill fully
var skill = await skillLoader.LoadSkillAsync("code-review");
// Result: Full instructions loaded, resources inventoried

// Level 3: Load resource content on demand
var template = await skillLoader.LoadResourceContentAsync(skill.Templates[0]);
// Result: Actual file content loaded
```

### SKILL.md Format

Each skill is defined in a `SKILL.md` file using YAML frontmatter:

```markdown
---
name: Code Review Assistant
description: Performs structured code reviews for quality and security
version: "1.0.0"
author: Your Name
category: development
tags:
  - code-review
  - quality
  - security
---

# Code Review Assistant

## Overview

This skill performs comprehensive code reviews...

## When to Use

Invoke this skill when:
- Reviewing pull requests
- Auditing code quality
- Checking for security vulnerabilities

## Process

1. Analyze the code structure
2. Check for common issues
3. Generate a report using the template

## Output Format

Use the template at `templates/review-report.template.md`
```

### Resource Management

Resources are organized into four standard folders:

| Folder | Purpose | Pattern | Example |
|--------|---------|---------|---------|
| `templates/` | Output formats | `*.template.*` | `report.template.md` |
| `references/` | Background docs | `*.md` | `coding-standards.md` |
| `scripts/` | Automation | `*.*` | `run-linter.py` |
| `assets/` | Static files | `*.*` | `schema.json` |

Resources are **discovered** at Level 2 but **loaded** only at Level 3, keeping memory usage low.

---

## Implementation Deep Dive

### SkillsCore Library

The `SkillsCore` project is a class library containing all the shared skill loading logic.

#### SkillDefinition Model

```csharp
public class SkillDefinition
{
    // Core metadata (from YAML frontmatter)
    public string Id { get; set; }           // Folder name
    public string Name { get; set; }         // Display name
    public string Description { get; set; }  // When to use
    public string? Instructions { get; set; } // Markdown body (Level 2+)

    // Extended metadata
    public string? Version { get; set; }
    public string? Author { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; }

    // Resources (discovered at Level 2)
    public List<SkillResource> Templates { get; set; }
    public List<SkillResource> References { get; set; }
    public List<SkillResource> Scripts { get; set; }
    public List<SkillResource> Assets { get; set; }

    // Loading state
    public bool IsFullyLoaded { get; set; }

    // Computed helpers
    public bool HasTemplates => Templates.Count > 0;
    public int TotalResourceCount => Templates.Count + References.Count + ...;
    public IEnumerable<SkillResource> AllResources => Templates.Concat(References)...;
}
```

#### SkillResource Model

```csharp
public class SkillResource
{
    public string FileName { get; set; }      // "report.template.md"
    public string FilePath { get; set; }      // Absolute path
    public string RelativePath { get; set; }  // "templates/report.template.md"
    public SkillResourceType ResourceType { get; set; }

    // Lazy loading
    public string? Content { get; set; }      // Null until loaded
    public bool IsLoaded { get; set; }        // Track load state

    // Metadata
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
}
```

### Skill Loading Service

The `SkillLoaderService` implements the `ISkillLoader` interface:

```csharp
public interface ISkillLoader
{
    // Level 1: Discovery
    Task<IReadOnlyList<SkillDefinition>> DiscoverSkillsAsync(CancellationToken ct = default);

    // Level 2: Full load
    Task<SkillDefinition?> LoadSkillAsync(string skillId, CancellationToken ct = default);

    // Level 3: Resource content
    Task<string?> LoadResourceContentAsync(SkillResource resource, CancellationToken ct = default);

    // Utilities
    Task<SkillDefinition?> GetSkillMetadataAsync(string skillId, CancellationToken ct = default);
    Task<IReadOnlyList<SkillDefinition>> FindSkillsByTagAsync(string tag, CancellationToken ct = default);
    void InvalidateCache();
}
```

#### Key Implementation Details

**YAML Frontmatter Parsing:**

```csharp
// Regex to separate YAML from markdown
[GeneratedRegex(@"^---\s*\n([\s\S]*?)\n---\s*\n([\s\S]*)", RegexOptions.Compiled)]
private static partial Regex FrontmatterRegex();

private (Dictionary<string, object>? Frontmatter, string? Body) ParseFrontmatter(string content)
{
    var match = FrontmatterRegex().Match(content);
    if (!match.Success) return (null, null);

    var yamlContent = match.Groups[1].Value;
    var body = match.Groups[2].Value;

    var frontmatter = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yamlContent);
    return (frontmatter, body);
}
```

**Two-Level Caching:**

```csharp
// Separate caches for different load levels
private readonly ConcurrentDictionary<string, SkillDefinition> _metadataCache = new();
private readonly ConcurrentDictionary<string, SkillDefinition> _fullCache = new();

// Cache validity tracking
private DateTime _lastDiscovery = DateTime.MinValue;
private readonly SemaphoreSlim _discoveryLock = new(1, 1);

private bool IsCacheValid() => DateTime.UtcNow - _lastDiscovery < _config.CacheDuration;
```

**Resource Discovery (without loading content):**

```csharp
private void DiscoverResourcesInFolder(
    SkillDefinition skill,
    List<SkillResource> resources,
    string folderName,
    string pattern,
    SkillResourceType resourceType)
{
    var folderPath = Path.Combine(skill.BaseDirectory, folderName);
    if (!Directory.Exists(folderPath)) return;

    // Assets support nested folders; others don't
    var searchOption = resourceType == SkillResourceType.Asset
        ? SearchOption.AllDirectories
        : SearchOption.TopDirectoryOnly;

    var files = Directory.GetFiles(folderPath, pattern, searchOption);

    foreach (var file in files)
    {
        var fileInfo = new FileInfo(file);
        resources.Add(new SkillResource
        {
            FileName = Path.GetFileName(file),
            FilePath = file,
            RelativePath = Path.GetRelativePath(skill.BaseDirectory, file),
            ResourceType = resourceType,
            IsLoaded = false,  // Content NOT loaded yet
            FileSize = fileInfo.Length,
            LastModified = fileInfo.LastWriteTimeUtc
        });
    }
}
```

### Configuration System

The `SkillsConfig` class provides configurable options:

```csharp
public class SkillsConfig
{
    public const string SectionName = "Skills";

    // Path configuration
    public string BasePath { get; set; } = "skills";
    public string SkillFileName { get; set; } = "SKILL.md";

    // Standard folder names (Claude Code convention)
    public string TemplatesDirectory { get; set; } = "templates";
    public string ReferencesDirectory { get; set; } = "references";
    public string ScriptsDirectory { get; set; } = "scripts";
    public string AssetsDirectory { get; set; } = "assets";

    // File patterns
    public string TemplatePattern { get; set; } = "*.template.*";
    public string ReferencePattern { get; set; } = "*.md";
    public string ScriptPattern { get; set; } = "*.*";
    public string AssetPattern { get; set; } = "*.*";

    // Caching
    public int CacheDurationMinutes { get; set; } = 5;
    public bool EagerLoadResources { get; set; } = false;

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(CacheDurationMinutes);
}
```

**appsettings.json:**

```json
{
  "Skills": {
    "BasePath": "skills",
    "SkillFileName": "SKILL.md",
    "CacheDurationMinutes": 5,
    "EagerLoadResources": false
  }
}
```

---

## MCP Server Integration

### What is MCP?

**Model Context Protocol (MCP)** is Anthropic's open standard for connecting AI models to external tools and data. It provides a standardized way for:

- **Servers** to expose tools, resources, and prompts
- **Clients** (Claude Desktop, Claude Code) to discover and use them

Think of MCP as a "USB standard for AI tools" - any MCP-compatible client can work with any MCP server.

```
┌─────────────────┐         ┌─────────────────┐
│   MCP Client    │  JSON   │   MCP Server    │
│                 │◄───────►│                 │
│  Claude Desktop │   RPC   │  Your .NET App  │
│  Claude Code    │ (stdio) │  with Tools     │
└─────────────────┘         └─────────────────┘
```

### MCP Tools Implementation

The `SkillsMcpServer` project exposes skills as MCP tools using the official .NET SDK.

**Program.cs - Server Setup:**

```csharp
var builder = Host.CreateApplicationBuilder(args);

// Load configuration
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false);

// Register skill loader
builder.Services.Configure<SkillsConfig>(
    builder.Configuration.GetSection(SkillsConfig.SectionName));
builder.Services.AddSingleton<ISkillLoader, SkillLoaderService>();

// Register MCP Server with stdio transport
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();  // Auto-discovers [McpServerTool] methods

await builder.Build().RunAsync();
```

**Skill Discovery Tools (SkillTools.cs):**

```csharp
[McpServerToolType]
public class SkillTools
{
    private readonly ISkillLoader _skillLoader;

    public SkillTools(ISkillLoader skillLoader)
    {
        _skillLoader = skillLoader;
    }

    [McpServerTool(Name = "list_skills")]
    [Description("Lists all available skills with their names and descriptions.")]
    public async Task<string> ListSkillsAsync(
        [Description("Optional tag to filter skills by")] string? tag = null,
        CancellationToken cancellationToken = default)
    {
        var skills = string.IsNullOrEmpty(tag)
            ? await _skillLoader.DiscoverSkillsAsync(cancellationToken)
            : await _skillLoader.FindSkillsByTagAsync(tag, cancellationToken);

        // Format results as markdown
        var output = new StringBuilder();
        output.AppendLine($"# Available Skills ({skills.Count})");

        foreach (var skill in skills)
        {
            output.AppendLine($"## {skill.Name} (`{skill.Id}`)");
            output.AppendLine(skill.Description);
            output.AppendLine($"**Tags:** {string.Join(", ", skill.Tags)}");
        }

        return output.ToString();
    }

    [McpServerTool(Name = "get_skill_details")]
    [Description("Gets full details about a skill including its instructions.")]
    public async Task<string> GetSkillDetailsAsync(
        [Description("The skill ID to get details for")] string skillId,
        CancellationToken cancellationToken = default)
    {
        var skill = await _skillLoader.LoadSkillAsync(skillId, cancellationToken);
        if (skill == null) return $"Skill '{skillId}' not found.";

        // Return full skill with instructions
        return $"# {skill.Name}\n\n{skill.Instructions}";
    }

    [McpServerTool(Name = "read_skill_resource")]
    [Description("Reads the content of a specific resource file from a skill.")]
    public async Task<string> ReadSkillResourceAsync(
        [Description("The skill ID")] string skillId,
        [Description("The relative path to the resource")] string resourcePath,
        CancellationToken cancellationToken = default)
    {
        var skill = await _skillLoader.LoadSkillAsync(skillId, cancellationToken);
        var resource = skill?.AllResources.FirstOrDefault(r =>
            r.RelativePath.Equals(resourcePath, StringComparison.OrdinalIgnoreCase));

        if (resource == null) return "Resource not found.";

        var content = await _skillLoader.LoadResourceContentAsync(resource, cancellationToken);
        return content ?? "Could not read resource.";
    }
}
```

**Presentation Tools (PresentationTools.cs):**

```csharp
[McpServerToolType]
public static class PresentationTools
{
    [McpServerTool(Name = "recommend_framework")]
    [Description("Recommends the best narrative framework for a presentation.")]
    public static string RecommendFramework(
        [Description("Is there a decision to be made?")] bool requiresDecision,
        [Description("Are the stakes high?")] bool hasHighStakes,
        [Description("Do you anticipate resistance?")] bool anticipatesResistance,
        [Description("Is this a transformation journey?")] bool isTransformation = false,
        [Description("Is the narrative opportunity-focused?")] bool isOpportunityFocused = false)
    {
        if (requiresDecision || hasHighStakes || anticipatesResistance)
        {
            return "**Recommended: SCR Framework**\n\n" +
                   "1. Situation (20%): Establish current state\n" +
                   "2. Complication (30%): Identify the problem\n" +
                   "3. Resolution (50%): Propose solution";
        }

        if (isTransformation)
            return "**Recommended: Past-Present-Future Framework**";

        if (isOpportunityFocused)
            return "**Recommended: Opportunity-Approach Framework**";

        return "**Recommended: Problem-Solution Framework**";
    }

    [McpServerTool(Name = "generate_assertive_headline")]
    [Description("Transforms weak topic labels into assertive headlines.")]
    public static string GenerateAssertiveHeadline(
        [Description("Weak topic label")] string topicLabel,
        [Description("Key insight")] string keyInsight,
        [Description("Supporting metric")] string? supportingMetric = null)
    {
        var assertive = topicLabel
            .Replace(" Overview", "")
            .Replace(" Summary", "")
            .Replace(" Analysis", "");

        return string.IsNullOrEmpty(supportingMetric)
            ? $"{assertive} {keyInsight}"
            : $"{assertive}: {keyInsight} ({supportingMetric})";
    }

    [McpServerTool(Name = "generate_slide_structure")]
    [Description("Generates a slide structure outline for a framework.")]
    public static string GenerateSlideStructure(
        [Description("Framework (SCR, PastPresentFuture, etc.)")] string framework,
        [Description("Presentation context")] string context,
        [Description("Number of slides")] int slideCount = 5)
    {
        // Returns detailed slide-by-slide breakdown
        // with headlines and key points for each slide
        // based on the chosen framework
    }

    [McpServerTool(Name = "create_presentation")]
    [Description("Creates a presentation from JSON slide definitions.")]
    public static string CreatePresentation(
        [Description("Title")] string title,
        [Description("Subtitle")] string subtitle,
        [Description("JSON slides array")] string slidesJson,
        [Description("Output path")] string outputPath = "presentation.pptx")
    {
        // Parses JSON and creates markdown outline
        // Could be extended to create actual PPTX files
    }
}
```

### Connecting to Claude Desktop

**1. Build the MCP server:**

```bash
cd SkillsQuickstart
dotnet build
```

**2. Configure Claude Desktop:**

Edit `%APPDATA%\Claude\claude_desktop_config.json` (Windows) or `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS):

```json
{
  "mcpServers": {
    "dotnet-skills": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\DotNetSkills\\SkillsQuickstart\\src\\SkillsMcpServer"
      ]
    }
  }
}
```

**3. Restart Claude Desktop**

**4. Use the tools:**

Once connected, Claude can use your tools:

> "What skills are available?"

Claude calls `list_skills` and returns:
```
# Available Skills (4)

## Code Review Assistant (`code-review`)
Performs structured code reviews...

## API Client Generator (`api-client`)
Generates API clients from OpenAPI specs...
```

> "Create a 5-slide deck recommending cloud migration"

Claude calls:
1. `recommend_framework(requiresDecision=true, hasHighStakes=true)` → SCR
2. `generate_slide_structure(framework="SCR", context="cloud migration", slideCount=5)`
3. `generate_assertive_headline(...)` for each slide
4. `create_presentation(...)` to output the result

---

## Building Your Own Skills

### Step 1: Create the folder structure

```
skills/
└── my-skill/
    ├── SKILL.md
    ├── templates/
    │   └── output.template.md
    └── references/
        └── guidelines.md
```

### Step 2: Write the SKILL.md

```markdown
---
name: My Custom Skill
description: Does something amazing. Use when you need amazing things done.
version: "1.0.0"
author: Your Name
category: custom
tags:
  - amazing
  - custom
---

# My Custom Skill

## Overview

This skill does amazing things by...

## When to Use

Invoke this skill when:
- You need amazing things
- Regular things won't do

## Process

1. First, analyze the input
2. Then, do the amazing thing
3. Finally, format the output using `templates/output.template.md`

## Important Notes

- Always check the guidelines in `references/guidelines.md`
- Never do un-amazing things
```

### Step 3: Add templates and references

**templates/output.template.md:**
```markdown
# {{title}}

## Summary
{{summary}}

## Details
{{details}}

---
Generated by My Custom Skill v1.0.0
```

**references/guidelines.md:**
```markdown
# Guidelines for Amazing Things

1. Be concise
2. Be accurate
3. Be amazing
```

### Step 4: Test it

```bash
dotnet run --project src/SkillsQuickstart
```

Your skill should appear in the discovery list.

---

## Running the Solution

### Prerequisites

- .NET 8.0 SDK
- (Optional) Claude Desktop for MCP integration

### Build

```bash
cd SkillsQuickstart
dotnet restore
dotnet build
```

### Run the Demo

```bash
dotnet run --project src/SkillsQuickstart
```

This shows the progressive disclosure pattern in action:
1. Level 1: Lists all skills with metadata
2. Level 2: Loads full instructions for a skill
3. Level 3: Loads resource content on demand

### Run the MCP Server

```bash
dotnet run --project src/SkillsMcpServer
```

The server starts and listens on stdio for MCP commands. Connect Claude Desktop to use it interactively.

---

## Real-World Example: Executive Deck Generator

The `ey-executive-deck` skill demonstrates a complete, production-ready skill:

**Purpose:** Create executive-level PowerPoint presentations

**Resources:**
- `references/narrative-framework.md` - SCR methodology guide
- `references/design-principles.md` - Slide design best practices
- `assets/brand/ey-brand.md` - Brand guidelines

**MCP Tools:**
- `recommend_framework` - Choose SCR vs Past-Present-Future vs Problem-Solution
- `generate_assertive_headline` - Convert "Market Overview" → "Market dynamics present $50M opportunity"
- `generate_slide_structure` - Create framework-specific slide outlines
- `create_presentation` - Generate the final output

**Example Interaction:**

User: "Create a 5-slide deck recommending Azure migration for CIO approval"

Claude:
1. Calls `recommend_framework(requiresDecision=true, hasHighStakes=true)` → Recommends SCR
2. Calls `read_skill_resource("ey-executive-deck", "references/narrative-framework.md")` → Gets SCR details
3. Calls `generate_slide_structure("SCR", "Azure migration recommendation", 5)` → Gets outline
4. Calls `generate_assertive_headline(...)` for each slide
5. Calls `create_presentation(...)` → Creates the deck

The entire workflow happens through standard MCP tool calls, with Claude orchestrating the process.

---

## Key Takeaways

1. **Skills are self-contained** - Everything needed is in one folder
2. **Progressive disclosure saves tokens** - Load only what you need
3. **MCP enables integration** - Standard protocol for AI tools
4. **Configuration is flexible** - Adapt to your needs
5. **Resources are lazy-loaded** - Content loads on demand

This pattern scales from simple documentation generators to complex multi-step workflows like the executive deck generator.

---

## Dependencies

- **SkillsCore:**
  - `Microsoft.Extensions.Options` (8.0.2)
  - `YamlDotNet` (16.2.0)

- **SkillsMcpServer:**
  - `ModelContextProtocol` (0.2.0-preview.1)
  - `Microsoft.Extensions.Hosting` (8.0.1)
  - `DocumentFormat.OpenXml` (3.0.1)

---

## License

MIT License - See LICENSE file for details.

---

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add your skill or enhancement
4. Submit a pull request

---

## Resources

- [Anthropic Claude Documentation](https://docs.anthropic.com)
- [Model Context Protocol Specification](https://modelcontextprotocol.io)
- [.NET MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk)
