# Skills Executor

A .NET orchestrator for executing Anthropic-style Skills with Azure OpenAI and MCP (Model Context Protocol) tool support.

## Overview

This project demonstrates how to build an AI agent orchestration system that:

1. **Loads Skills** - Parses `SKILL.md` files (Anthropic's skill format) with YAML frontmatter
2. **Connects to MCP Servers** - Acts as an MCP client to discover and execute tools
3. **Orchestrates LLM Calls** - Uses Azure OpenAI with function calling in an agentic loop
4. **Routes Tool Calls** - Bridges between Azure OpenAI tool calls and MCP server execution

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Skills Executor                               │
│                     (.NET Console Application)                       │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│   ┌──────────────┐    ┌──────────────┐    ┌──────────────────────┐  │
│   │ Skill Loader │    │ Azure OpenAI │    │   MCP Client Service │  │
│   │              │    │   Service    │    │                      │  │
│   │ • Discovers  │    │              │    │ • Connects to MCP    │  │
│   │   SKILL.md   │    │ • Chat API   │    │   servers            │  │
│   │ • Parses     │    │ • Function   │    │ • Routes tool calls  │  │
│   │   YAML       │    │   Calling    │    │ • Returns results    │  │
│   └──────────────┘    └──────────────┘    └──────────────────────┘  │
│                                                    │                 │
└────────────────────────────────────────────────────│─────────────────┘
                                                     │
                           ┌─────────────────────────┴─────────────────────────┐
                           │                                                   │
                           ▼                                                   ▼
              ┌────────────────────────┐                         ┌─────────────────────────┐
              │   Skills MCP Server    │                         │  External MCP Servers   │
              │   (Custom .NET)        │                         │  (e.g., GitHub)         │
              │                        │                         │                         │
              │ • analyze_directory    │                         │ • search_repositories   │
              │ • count_lines          │                         │ • list_issues           │
              │ • find_patterns        │                         │ • get_file_contents     │
              └────────────────────────┘                         └─────────────────────────┘
```

## Project Structure

```
SkillsQuickstart/
├── src/
│   ├── SkillsCore/              # Shared library
│   │   ├── Config/              # Configuration models
│   │   ├── Models/              # SkillDefinition, SkillResource
│   │   └── Services/            # ISkillLoader, SkillLoaderService
│   │
│   ├── SkillsQuickstart/        # Main orchestrator application
│   │   ├── Config/              # AzureOpenAIConfig, McpServerConfig
│   │   ├── Services/            # AzureOpenAI, MCP Client, Skill Executor
│   │   ├── skills/              # SKILL.md files
│   │   │   ├── code-explainer/  # Skill 1: No tools (pure LLM)
│   │   │   ├── project-analyzer/# Skill 2: Custom MCP tools
│   │   │   └── github-assistant/# Skill 3: External MCP server
│   │   ├── Program.cs           # Entry point with Spectre.Console UI
│   │   └── appsettings.json     # Configuration
│   │
│   └── SkillsMcpServer/         # Custom MCP server
│       ├── Tools/               # Tool implementations
│       │   └── ProjectAnalysisTools.cs
│       └── Program.cs           # MCP server entry point
│
└── README.md
```

## Skills

Skills are markdown files with YAML frontmatter that define the system prompt for the LLM:

### 1. Code Explainer (No Tools)
Pure LLM reasoning - explains code without any tool usage.

```yaml
---
name: Code Explainer
description: Explains code in plain English...
tags: [code, explanation, no-tools]
---
# Instructions...
```

### 2. Project Analyzer (Custom MCP Tools)
Uses custom tools from our `SkillsMcpServer`:

| Tool | Description |
|------|-------------|
| `analyze_directory` | Returns directory tree with file sizes |
| `count_lines` | Counts lines of code by file type |
| `find_patterns` | Finds TODO, FIXME, HACK patterns |

### 3. GitHub Assistant (External MCP Server)
Uses the official `@modelcontextprotocol/server-github` MCP server:

| Tool | Description |
|------|-------------|
| `search_repositories` | Search GitHub repos |
| `list_issues` | List issues in a repo |
| `get_file_contents` | Get file contents |
| `search_code` | Search code across GitHub |

## Prerequisites

- .NET 8.0 SDK
- Node.js (for external MCP servers like GitHub)
- Azure OpenAI resource with a GPT-4 deployment

## Configuration

### 1. Set up User Secrets

```bash
cd src/SkillsQuickstart

# Azure OpenAI credentials
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"

# GitHub Personal Access Token (for github-assistant skill)
dotnet user-secrets set "GITHUB_PERSONAL_ACCESS_TOKEN" "your-githbu-access-token"
```

### 2. Configure MCP Servers

Edit `appsettings.json` to configure MCP servers:

```json
{
  "McpServers": {
    "Servers": [
      {
        "Name": "skills-mcp-server",
        "Command": "dotnet",
        "Arguments": ["path/to/SkillsMcpServer.dll"],
        "Environment": {},
        "Enabled": true
      },
      {
        "Name": "github-mcp-server",
        "Command": "npx",
        "Arguments": ["-y", "@modelcontextprotocol/server-github"],
        "Environment": {
          "GITHUB_PERSONAL_ACCESS_TOKEN": ""
        },
        "Enabled": true
      }
    ]
  }
}
```

> **Note**: Empty environment values are resolved from User Secrets at runtime.

## Running the Application

### Build the MCP Server first

```bash
cd src/SkillsMcpServer
dotnet build
```

### Run the Orchestrator

```bash
cd src/SkillsQuickstart
dotnet run
```

The application will:
1. Display available skills
2. Connect to configured MCP servers
3. Show available tools
4. Let you select a skill
5. Accept your input
6. Execute the agentic loop (LLM -> Tools -> LLM -> ...)
7. Display results

## How It Works

### Orchestration Flow

```
User Input
    │
    ▼
┌─────────────────────────────────────────────┐
│ Skill Executor                              │
│                                             │
│  1. Build system prompt from SKILL.md       │
│  2. Initialize conversation                 │
│  3. Get available tools from MCP servers    │
│                                             │
│  ┌─────────────────────────────────────┐   │
│  │ Agentic Loop (max 10 turns)         │   │
│  │                                     │   │
│  │  Call Azure OpenAI ──────────────┐  │   │
│  │       │                          │  │   │
│  │       ▼                          │  │   │
│  │  Tool calls?                     │  │   │
│  │    │     │                       │  │   │
│  │   Yes    No ─────► Return        │  │   │
│  │    │            Response         │  │   │
│  │    ▼                             │  │   │
│  │  Execute via MCP ────────────────┘  │   │
│  │  Add results to conversation        │   │
│  └─────────────────────────────────────┘   │
│                                             │
└─────────────────────────────────────────────┘
```

### How Tool Selection Works

A key architectural principle: **the LLM decides which tools to use, not the orchestrator**.

The orchestrator has zero hardcoded logic about when to call specific tools. Instead, it provides the LLM with:

1. **Skill instructions** (system prompt from SKILL.md)
2. **Available tools** (function definitions with descriptions)
3. **User input** (the user's request)

The LLM reasons about all three and decides whether to call tools and which ones.

```
┌─────────────────────────────────────────────────────┐
│ What the LLM Receives                               │
├─────────────────────────────────────────────────────┤
│ System Prompt: (from SKILL.md)                      │
│   "You are a project analyzer. Use these tools:     │
│    - analyze_directory: shows file structure        │
│    - count_lines: counts lines of code              │
│    - find_patterns: finds TODOs/FIXMEs"             │
│                                                     │
│ Tools: [                                            │
│   { name: "analyze_directory", description: "...",  │
│     parameters: { path: string, maxDepth: int } },  │
│   { name: "count_lines", ... },                     │
│   ...                                               │
│ ]                                                   │
│                                                     │
│ User Message: "Analyze the src folder"              │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
              LLM reasons and decides:
      "I should call analyze_directory first..."
```

**Skills guide tool selection through their instructions:**

| Guidance Level | Example | Result |
|----------------|---------|--------|
| **No mention** | Code Explainer skill has no tool instructions | LLM uses pure reasoning, no tools |
| **Suggested** | "You can use analyze_directory to see structure" | LLM may or may not use tools |
| **Required** | "ALWAYS call tools. Do not make assumptions." | LLM will use tools for every request |

**The agentic loop in SkillExecutor.cs:**

```csharp
while (turnCount < maxTurns)
{
    // Call Azure OpenAI - LLM decides what to do
    var result = await _openAIService.GetCompletionAsync(messages, tools);

    if (result.HasToolCalls)
    {
        // LLM requested tools - orchestrator just executes them
        foreach (var toolCall in result.ToolCalls)
        {
            var toolResult = await _mcpClientService.ExecuteToolAsync(
                toolCall.FunctionName,
                toolCall.FunctionArguments.ToString());

            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
        }
        continue; // Let LLM see results and decide next action
    }

    // No tool calls - LLM is done, return response
    return new SkillExecutionResult { Response = result.TextResponse };
}
```

The orchestrator is "dumb plumbing" - it executes whatever the LLM requests and feeds results back. The intelligence is entirely in the LLM's reasoning.

### MCP Client Service

The `McpClientService` acts as an MCP client that:

1. **Initializes** - Spawns MCP server processes via stdio transport
2. **Discovers** - Lists available tools from each server
3. **Routes** - Maps tool names to their source server
4. **Executes** - Calls tools and returns results

```csharp
// Example: Execute a tool
var result = await mcpClientService.ExecuteToolAsync(
    "analyze_directory",
    """{"path": "C:/projects/myapp", "maxDepth": 3}""");
```

### Skill Definition Format

Skills use Anthropic's `SKILL.md` format:

```markdown
---
name: My Skill
description: What this skill does and when to use it.
version: 1.0.0
author: Your Name
category: development
tags:
  - tag1
  - tag2
---

# Skill Instructions

Detailed instructions that become the system prompt...

## Available Tools
- tool_1: Description
- tool_2: Description

## How to Help Users
...
```

## Creating New Skills

1. Create a folder under `skills/` with your skill name (e.g., `my-skill/`)
2. Add a `SKILL.md` file with YAML frontmatter
3. Write clear instructions for the LLM
4. Document which tools (if any) the skill should use

## Creating Custom MCP Tools

Add tools to `SkillsMcpServer/Tools/`:

```csharp
using ModelContextProtocol.Server;

[McpServerToolType]
public static class MyTools
{
    [McpServerTool(Name = "my_tool")]
    [Description("What this tool does")]
    public static string MyTool(
        [Description("Parameter description")] string param1)
    {
        // Implementation
        return "Result";
    }
}
```

## Key Concepts

### Skills vs Tools

| Concept | Description |
|---------|-------------|
| **Skill** | A `SKILL.md` file that defines the system prompt and instructions for the LLM |
| **Tool** | A function exposed via MCP that the LLM can call to perform actions |

### MCP Architecture

- **MCP Server**: Exposes tools via the Model Context Protocol
- **MCP Client**: Connects to servers, discovers tools, executes calls
- **Stdio Transport**: Communication via stdin/stdout (spawned processes)

## Dependencies

| Package | Purpose |
|---------|---------|
| `Azure.AI.OpenAI` | Azure OpenAI client |
| `ModelContextProtocol` | MCP client/server SDK |
| `Spectre.Console` | Rich terminal UI |
| `YamlDotNet` | YAML frontmatter parsing |

## Troubleshooting

### MCP Server won't connect

1. Ensure the MCP server is built: `dotnet build` in `SkillsMcpServer/`
2. Check the path in `appsettings.json` is absolute
3. Verify Node.js is installed (for external servers like GitHub)

### Azure OpenAI errors

1. Verify credentials in User Secrets
2. Check the deployment name matches your Azure resource
3. Ensure your API key has access to the deployment

### GitHub tools not working

1. Verify `GITHUB_PERSONAL_ACCESS_TOKEN` is set in User Secrets
2. Ensure the token has appropriate scopes (repo, read:org)
3. Check rate limits if making many requests

## License

MIT
