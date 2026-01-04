---
name: Documentation Generator
description: Creates comprehensive project documentation including READMEs, API docs, and architecture guides. Use when documenting new projects or updating existing docs.
version: "1.0.0"
author: Skills Quickstart
category: documentation
tags:
  - documentation
  - readme
  - technical-writing
---
# Documentation Generator

## Overview

This skill generates clear, comprehensive documentation for software projects. It follows industry best practices for technical writing and ensures documentation stays maintainable over time.

## When to Use This Skill

Invoke this skill when:
- Setting up documentation for a new project
- Creating or updating README files
- Documenting APIs or libraries
- Writing architecture decision records (ADRs)
- Generating changelog entries

## Documentation Types

### 1. README Files

Primary entry point for any project. Use `templates/readme.template.md` for consistent structure.

Required sections:
- Project title and description
- Prerequisites and installation
- Quick start / basic usage
- Configuration options
- Contributing guidelines
- License information

### 2. API Documentation

For libraries and services:
- Endpoint/method descriptions
- Parameter documentation
- Request/response examples
- Error codes and handling
- Authentication requirements

### 3. Architecture Documentation

For complex systems:
- System overview diagrams
- Component responsibilities
- Data flow descriptions
- Technology choices (ADRs)
- Deployment architecture

## Instructions

### Step 1: Analyze the Codebase

1. Identify the project type (library, service, application)
2. Find existing documentation
3. Review public APIs and entry points
4. Note configuration options
5. Check for existing patterns/conventions

### Step 2: Choose Documentation Type

Based on analysis:
- New project → Full README
- API/Library → API docs + README
- Complex system → Architecture docs + README
- Update request → Targeted updates only

### Step 3: Generate Documentation

1. Use appropriate template from `templates/`
2. Fill in project-specific details
3. Include code examples where helpful
4. Add diagrams for complex concepts
5. Ensure links are valid

### Step 4: Review and Refine

- Check for accuracy against code
- Verify examples work
- Ensure consistent terminology
- Test installation instructions

## Writing Guidelines

### Tone and Style

- Use active voice: "Run the command" not "The command should be run"
- Be concise but complete
- Address the reader directly with "you"
- Use present tense for descriptions

### Formatting

- Use headers to create scannable structure
- Include code blocks for all commands/code
- Use bullet points for lists
- Add tables for structured data

### Code Examples

```csharp
// Good: Show complete, runnable examples
var client = new ApiClient("https://api.example.com");
var result = await client.GetUsersAsync();

// Bad: Partial snippets that can't be run
GetUsersAsync(); // ???
```

## Templates

| Template | Use Case |
|----------|----------|
| `readme.template.md` | Project README files |

## Quality Checklist

- [ ] Title clearly describes the project
- [ ] Purpose is explained in first paragraph
- [ ] Prerequisites are listed
- [ ] Installation steps are complete and tested
- [ ] Basic usage example is provided
- [ ] All code examples are tested
- [ ] Links are valid
- [ ] License is specified
