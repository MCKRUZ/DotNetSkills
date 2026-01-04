---
name: API Client Generator
description: Generates strongly-typed API clients from OpenAPI specifications. Use when integrating with REST APIs or creating SDK wrappers.
version: "1.0.0"
author: Skills Quickstart
category: development
tags:
  - development
  - api
  - code-generation
  - openapi
---
# API Client Generator

## Overview

This skill generates strongly-typed API client code from OpenAPI (Swagger) specifications. It produces clean, maintainable client classes that follow .NET best practices.

## When to Use This Skill

Invoke this skill when:
- Integrating with a new REST API
- Updating an existing API client after spec changes
- Creating SDK wrappers for internal services
- Generating TypeScript clients for frontend consumption

## Prerequisites

- OpenAPI 3.0+ specification (JSON or YAML)
- Target framework (.NET 6+, TypeScript, etc.)
- Desired HTTP client library (HttpClient, Refit, etc.)

## Instructions

### Step 1: Analyze the OpenAPI Spec

1. Load the OpenAPI specification from `assets/openapi-schema.json` or a provided URL
2. Identify all endpoints, grouped by tag/controller
3. Extract request/response models
4. Note authentication requirements

### Step 2: Generate Models

For each schema definition:

```csharp
// Generate record for immutable DTOs
public record {ModelName}
{
    // Map OpenAPI properties to C# properties
    // Use appropriate nullability based on 'required' array
}
```

### Step 3: Generate Client Class

Use the template at `templates/client-class.template.cs`:

1. Create one client class per API tag/controller
2. Inject HttpClient via constructor
3. Include XML documentation from OpenAPI descriptions
4. Handle authentication headers
5. Implement proper error handling

### Step 4: Generate Script (Optional)

If regeneration automation is needed, use `scripts/generate-client.py` as a reference for creating a build-time generation script.

## Output Structure

```
Generated/
├── Models/
│   ├── {ModelName}.cs
│   └── ...
├── Clients/
│   ├── {Tag}Client.cs
│   └── ...
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

## Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `namespace` | `Api.Client` | Root namespace for generated code |
| `nullable` | `true` | Enable nullable reference types |
| `records` | `true` | Use records for DTOs |
| `cancellation` | `true` | Include CancellationToken parameters |

## Best Practices

1. **Version your specs**: Keep OpenAPI specs in source control
2. **Regenerate on build**: Use MSBuild targets for automation
3. **Don't modify generated code**: Use partial classes for extensions
4. **Include generated code in repo**: For build reproducibility
