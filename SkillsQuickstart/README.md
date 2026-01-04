# Agent Skills Loader Quickstart

A .NET 8 implementation of Anthropic's Agent Skills pattern, demonstrating how to structure, discover, and load skills for AI agents.

## What This Demonstrates

This quickstart shows how to implement the **Agent Skills pattern** used by Claude Code and similar AI agent systems. The key concepts are:

1. **Skill Structure** - Matching Claude Code's conventions exactly
2. **YAML Frontmatter** - Parsing metadata from SKILL.md files
3. **Progressive Disclosure** - Loading only what's needed, when it's needed
4. **Resource Management** - Bundling templates, references, scripts, and assets

## Folder Structure

Skills follow Claude Code's standard folder conventions:

```
skills/
└── my-skill/
    ├── SKILL.md              # Required: frontmatter + instructions
    ├── templates/            # Optional: output templates
    │   └── *.template.*
    ├── references/           # Optional: reference documentation
    │   └── *.md
    ├── scripts/              # Optional: executable scripts
    │   └── *.*
    └── assets/               # Optional: binary files, images, etc.
        └── *.*
```

### SKILL.md Format

Each skill requires a `SKILL.md` file with YAML frontmatter:

```yaml
---
name: My Skill Name
description: Brief description of what this skill does and when to use it.
version: "1.0.0"
author: Your Name
category: development
tags:
  - tag1
  - tag2
---
# Skill Instructions

Detailed markdown instructions for the AI agent...
```

## Progressive Disclosure

The loader implements three levels of progressive disclosure to minimize memory usage and load times:

| Level | Method | What's Loaded | Use Case |
|-------|--------|---------------|----------|
| 1 | `DiscoverSkillsAsync()` | Metadata only (name, description, tags, resource counts) | Displaying skill catalogs/lists |
| 2 | `LoadSkillAsync()` | Full skill with instructions + resource inventory | Preparing to use a skill |
| 3 | `LoadResourceContentAsync()` | Actual file content | Using a specific template/reference |

```csharp
// Level 1: Quick discovery for listing
var skills = await skillLoader.DiscoverSkillsAsync();
// skills[0].Instructions == null (not loaded yet)

// Level 2: Full load when skill is selected
var skill = await skillLoader.LoadSkillAsync("code-review");
// skill.Instructions contains markdown body
// skill.Templates has file metadata (but not content)

// Level 3: Load resource content on demand
var template = skill.Templates.First();
var content = await skillLoader.LoadResourceContentAsync(template);
// Now template.Content and template.IsLoaded are populated
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022, VS Code, or Rider

### Running the Demo

```bash
cd SkillsQuickstart
dotnet restore
dotnet run --project src/SkillsQuickstart
```

### Expected Output

```
╔══════════════════════════════════════════════════════════════╗
║       Agent Skills Loader - Progressive Disclosure Demo      ║
╚══════════════════════════════════════════════════════════════╝

═══════════════════════════════════════════════════════════════
 LEVEL 1: Discovery (Metadata Only)
═══════════════════════════════════════════════════════════════

┌─ Code Review Assistant (code-review)
│  Performs structured code reviews...
│  Tags: [development, quality, security, code-review]
│  Resources: 2 files
│  Instructions Loaded: False
└─ Fully Loaded: False

...
```

## Project Structure

```
SkillsQuickstart/
├── SkillsQuickstart.sln
├── README.md
└── src/
    └── SkillsQuickstart/
        ├── SkillsQuickstart.csproj
        ├── Program.cs                    # Demo entry point
        ├── appsettings.json              # Configuration
        ├── Config/
        │   └── SkillsConfig.cs           # IOptions configuration
        ├── Models/
        │   ├── SkillDefinition.cs        # Skill representation
        │   ├── SkillResource.cs          # Resource representation
        │   └── SkillResourceType.cs      # Resource categories
        ├── Services/
        │   ├── ISkillLoader.cs           # Service interface
        │   └── SkillLoaderService.cs     # Implementation
        └── skills/                        # Example skills
            ├── code-review/
            ├── api-client/
            └── documentation/
```

## How to Create Skills

### 1. Create the Skill Folder

```bash
mkdir skills/my-new-skill
```

### 2. Create SKILL.md

```markdown
---
name: My New Skill
description: What this skill does and when to use it.
tags:
  - relevant-tag
---
# My New Skill

## Instructions

Tell the AI agent how to use this skill...
```

### 3. Add Resources (Optional)

```bash
# Add output templates
mkdir skills/my-new-skill/templates
echo "# Report..." > skills/my-new-skill/templates/report.template.md

# Add reference documentation
mkdir skills/my-new-skill/references
echo "# Guidelines..." > skills/my-new-skill/references/guidelines.md
```

### 4. Discovery is Automatic

The loader automatically discovers new skills on the next `DiscoverSkillsAsync()` call.

## Configuration

Configuration is managed via `appsettings.json`:

```json
{
  "Skills": {
    "BasePath": "skills",
    "SkillFileName": "SKILL.md",
    "TemplatesDirectory": "templates",
    "ReferencesDirectory": "references",
    "ScriptsDirectory": "scripts",
    "AssetsDirectory": "assets",
    "CacheDurationMinutes": 5,
    "EagerLoadResources": false
  }
}
```

| Option | Default | Description |
|--------|---------|-------------|
| `BasePath` | `skills` | Root directory for skill discovery |
| `SkillFileName` | `SKILL.md` | Name of skill definition file |
| `CacheDurationMinutes` | `5` | How long to cache discovered skills |
| `EagerLoadResources` | `false` | If true, loads all resource content during skill load |

## Example Skills Included

### code-review
Demonstrates: Templates + References

Performs structured code reviews with security, quality, performance, and maintainability analysis.

### api-client
Demonstrates: Templates + Scripts + Assets

Generates strongly-typed API clients from OpenAPI specifications.

### documentation
Demonstrates: Templates only

Creates comprehensive project documentation including READMEs and guides.

## Key Implementation Details

### Frontmatter Parsing

The service uses regex to separate YAML frontmatter from markdown:

```csharp
[GeneratedRegex(@"^---\s*\n([\s\S]*?)\n---\s*\n([\s\S]*)")]
private static partial Regex FrontmatterRegex();
```

### Two-Level Caching

```csharp
// Metadata cache: Only frontmatter loaded
private readonly ConcurrentDictionary<string, SkillDefinition> _metadataCache;

// Full cache: Frontmatter + instructions loaded
private readonly ConcurrentDictionary<string, SkillDefinition> _fullCache;
```

### Resource Discovery (Not Loading)

Resources are discovered (paths captured) without reading file content:

```csharp
private void DiscoverResources(SkillDefinition skill)
{
    DiscoverResourcesInFolder(skill, skill.Templates,
        "templates", "*.template.*", SkillResourceType.Template);
    // ... other folders
}
```

## Extending for Production

This quickstart demonstrates the core pattern. For production use, consider:

- **Dependency Management**: Skills that depend on other skills
- **Workflow Orchestration**: Multi-skill workflows with phases
- **AI Provider Integration**: Injecting skills into agent prompts
- **Version Management**: Supporting multiple skill versions
- **Hot Reloading**: Watching for skill file changes
- **Validation**: Schema validation for skill frontmatter

## Dependencies

- `YamlDotNet` - YAML frontmatter parsing
- `Microsoft.Extensions.Configuration` - Configuration management
- `Microsoft.Extensions.DependencyInjection` - Dependency injection
- `Microsoft.Extensions.Options` - IOptions pattern support

## License

This project is provided as a reference implementation for educational purposes.
