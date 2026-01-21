using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SkillsCore.Config;
using SkillsCore.Models;
using SkillsCore.Services;
using SkillsQuickstart.Config;
using SkillsQuickstart.Services;
using Spectre.Console;

// ═══════════════════════════════════════════════════════════════════════════
// Skills Executor - .NET Orchestrator for Anthropic Skills
// ═══════════════════════════════════════════════════════════════════════════

AnsiConsole.Write(
    new FigletText("Skills Executor")
        .LeftJustified()
        .Color(Color.Cyan1));

AnsiConsole.MarkupLine("[dim].NET Orchestrator for Anthropic Skills Pattern[/]\n");

// ─────────────────────────────────────────────────────────────────────────────
// Setup: Configuration and Dependency Injection
// ─────────────────────────────────────────────────────────────────────────────

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<AzureOpenAIConfig>()
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.Configure<SkillsConfig>(configuration.GetSection(SkillsConfig.SectionName));
services.Configure<AzureOpenAIConfig>(configuration.GetSection(AzureOpenAIConfig.SectionName));
services.Configure<McpServersConfig>(configuration.GetSection(McpServersConfig.SectionName));

services.AddSingleton<ISkillLoader, SkillLoaderService>();
services.AddSingleton<IAzureOpenAIService, AzureOpenAIService>();
services.AddSingleton<IMcpClientService, McpClientService>();
services.AddSingleton<ISkillExecutor, SkillExecutor>();

var serviceProvider = services.BuildServiceProvider();

var skillLoader = serviceProvider.GetRequiredService<ISkillLoader>();
var mcpClientService = serviceProvider.GetRequiredService<IMcpClientService>();
var skillExecutor = serviceProvider.GetRequiredService<ISkillExecutor>();

var azureConfig = serviceProvider.GetRequiredService<IOptions<AzureOpenAIConfig>>().Value;
var mcpConfig = serviceProvider.GetRequiredService<IOptions<McpServersConfig>>().Value;

// Show configuration
var configTable = new Table()
    .Border(TableBorder.Rounded)
    .AddColumn("[bold]Setting[/]")
    .AddColumn("[bold]Value[/]");

configTable.AddRow("Azure OpenAI Endpoint", string.IsNullOrEmpty(azureConfig.Endpoint) ? "[red]Not configured[/]" : $"[green]{azureConfig.Endpoint}[/]");
configTable.AddRow("Model", azureConfig.DeploymentName);
configTable.AddRow("MCP Servers", $"{mcpConfig.Servers.Count(s => s.Enabled)} configured");

AnsiConsole.Write(new Panel(configTable).Header("[bold cyan]Configuration[/]").BorderColor(Color.Grey));
AnsiConsole.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 1: Discover Available Skills
// ─────────────────────────────────────────────────────────────────────────────

IReadOnlyList<SkillDefinition> skills = null!;

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .StartAsync("[cyan]Discovering skills...[/]", async ctx =>
    {
        skills = await skillLoader.DiscoverSkillsAsync();
    });

if (skills.Count == 0)
{
    AnsiConsole.MarkupLine("[red]No skills found.[/] Ensure the 'skills' folder exists with SKILL.md files.");
    return;
}

// Show skills table
var skillsTable = new Table()
    .Border(TableBorder.Rounded)
    .AddColumn("[bold]Skill[/]")
    .AddColumn("[bold]Description[/]")
    .AddColumn("[bold]Tags[/]");

foreach (var skill in skills)
{
    skillsTable.AddRow(
        $"[cyan]{skill.Name}[/]",
        skill.Description.Length > 60 ? skill.Description[..60] + "..." : skill.Description,
        $"[dim]{string.Join(", ", skill.Tags.Take(3))}[/]"
    );
}

AnsiConsole.Write(new Panel(skillsTable).Header($"[bold green]Available Skills ({skills.Count})[/]").BorderColor(Color.Green));
AnsiConsole.WriteLine();

// ─────────────────────────────────────────────────────────────────────────────
// Step 2: Connect to MCP Servers
// ─────────────────────────────────────────────────────────────────────────────

var toolCount = 0;
try
{
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("[cyan]Connecting to MCP servers...[/]", async ctx =>
        {
            await mcpClientService.InitializeAsync();
        });

    var tools = mcpClientService.GetAvailableTools();
    toolCount = tools.Count;

    var toolsTable = new Table()
        .Border(TableBorder.Simple)
        .AddColumn("[bold]Tool[/]")
        .AddColumn("[bold]Description[/]");

    foreach (var tool in tools)
    {
        var desc = tool.FunctionDescription ?? "";
        toolsTable.AddRow(
            $"[yellow]{tool.FunctionName}[/]",
            desc.Length > 50 ? desc[..50] + "..." : desc
        );
    }

    AnsiConsole.Write(new Panel(toolsTable).Header($"[bold yellow]MCP Tools ({tools.Count})[/]").BorderColor(Color.Yellow));
    AnsiConsole.WriteLine();
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[yellow]Warning:[/] Could not connect to MCP servers: {ex.Message}");
    AnsiConsole.MarkupLine("[dim]Continuing without tool support...[/]\n");
}

// ─────────────────────────────────────────────────────────────────────────────
// Main Loop: Select and Execute Skills
// ─────────────────────────────────────────────────────────────────────────────

var running = true;
while (running)
{
    // Step 3: Select a Skill
    var selectedSkill = AnsiConsole.Prompt(
        new SelectionPrompt<SkillDefinition>()
            .Title("[bold]Select a skill to execute:[/]")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more skills)[/]")
            .UseConverter(s => $"{s.Name} [dim]({s.Id})[/]")
            .AddChoices(skills));

    // Load full skill
    SkillDefinition? loadedSkill = null;
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync($"[cyan]Loading {selectedSkill.Name}...[/]", async ctx =>
        {
            loadedSkill = await skillLoader.LoadSkillAsync(selectedSkill.Id);
        });

    if (loadedSkill == null)
    {
        AnsiConsole.MarkupLine($"[red]Failed to load skill '{selectedSkill.Id}'.[/]");
        continue;
    }

    AnsiConsole.Write(new Rule($"[bold cyan]{loadedSkill.Name}[/]").LeftJustified());
    AnsiConsole.MarkupLine($"[dim]{loadedSkill.Description}[/]");
    AnsiConsole.MarkupLine($"Instructions: [green]{loadedSkill.Instructions?.Length ?? 0}[/] characters | Resources: [green]{loadedSkill.TotalResourceCount}[/] files\n");

    // Step 4: Execute the Skill
    if (string.IsNullOrEmpty(azureConfig.ApiKey) || string.IsNullOrEmpty(azureConfig.Endpoint))
    {
        AnsiConsole.Write(new Panel(
            new Markup("[red]Azure OpenAI not configured.[/]\n\n" +
                       "Set your credentials using User Secrets:\n" +
                       "[dim]dotnet user-secrets set \"AzureOpenAI:Endpoint\" \"https://your-resource.openai.azure.com/\"[/]\n" +
                       "[dim]dotnet user-secrets set \"AzureOpenAI:ApiKey\" \"your-api-key\"[/]"))
            .Header("[bold red]Configuration Required[/]")
            .BorderColor(Color.Red));
    }
    else
    {
        var userInput = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold]Enter your request:[/]")
                .PromptStyle("green"));

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Execution Log[/]").LeftJustified());
        AnsiConsole.WriteLine();

        // Execute without spinner so we can see live progress
        var result = await skillExecutor.ExecuteAsync(loadedSkill, userInput);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Results[/]").LeftJustified());

        if (result != null)
        {
            // Results panel
            var resultPanel = new Table()
                .Border(TableBorder.None)
                .HideHeaders()
                .AddColumn("")
                .AddColumn("");

            resultPanel.AddRow("[bold]Status[/]", result.Success ? "[green]Success[/]" : "[red]Failed[/]");
            resultPanel.AddRow("[bold]Turns[/]", result.TurnCount.ToString());
            resultPanel.AddRow("[bold]Tool Calls[/]", result.ToolCalls.Count.ToString());

            if (!string.IsNullOrEmpty(result.Error))
            {
                resultPanel.AddRow("[bold]Error[/]", $"[red]{Markup.Escape(result.Error)}[/]");
            }

            AnsiConsole.Write(new Panel(resultPanel).Header("[bold]Execution Summary[/]").BorderColor(Color.Blue));

            // Tool calls
            if (result.ToolCalls.Count > 0)
            {
                var toolCallsTable = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("[bold]Tool[/]")
                    .AddColumn("[bold]Result Preview[/]");

                foreach (var toolCall in result.ToolCalls)
                {
                    var resultPreview = toolCall.Result.Length > 80
                        ? toolCall.Result[..80] + "..."
                        : toolCall.Result;
                    toolCallsTable.AddRow(
                        $"[yellow]{Markup.Escape(toolCall.ToolName)}[/]",
                        Markup.Escape(resultPreview)
                    );
                }

                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Panel(toolCallsTable).Header("[bold yellow]Tool Calls Executed[/]").BorderColor(Color.Yellow));
            }

            // Response
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel(
                new Markup(Markup.Escape(result.Response)))
                .Header("[bold green]Response[/]")
                .BorderColor(Color.Green)
                .Expand());
        }
    }

    // Ask if user wants to continue
    AnsiConsole.WriteLine();
    running = AnsiConsole.Confirm("[bold]Run another skill?[/]", defaultValue: true);
    AnsiConsole.WriteLine();
}

// Cleanup
if (mcpClientService is IAsyncDisposable disposable)
{
    await disposable.DisposeAsync();
}
