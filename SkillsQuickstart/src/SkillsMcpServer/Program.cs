using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;
using SkillsCore.Config;
using SkillsCore.Services;

// ═══════════════════════════════════════════════════════════════════════════
// Skills MCP Server
// Exposes Anthropic-style skills as MCP tools
// ═══════════════════════════════════════════════════════════════════════════

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
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();
