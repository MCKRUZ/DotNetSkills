using System.Text.Json;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI.Chat;
using SkillsQuickstart.Config;

namespace SkillsQuickstart.Services;

/// <summary>
/// Service for managing MCP server connections and executing tool calls.
/// Routes tool calls from Azure OpenAI to the appropriate MCP server.
/// </summary>
public class McpClientService : IMcpClientService
{
    private readonly McpServersConfig _config;
    private readonly Dictionary<string, IMcpClient> _clients = new();
    private readonly Dictionary<string, (string ServerName, McpClientTool Tool)> _toolRegistry = new();
    private bool _initialized;

    public McpClientService(IOptions<McpServersConfig> config)
    {
        _config = config.Value;
    }

    /// <summary>
    /// Initializes connections to all configured MCP servers.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
            return;

        foreach (var serverConfig in _config.Servers.Where(s => s.Enabled))
        {
            try
            {
                Console.WriteLine($"  Connecting to MCP server: {serverConfig.Name}...");

                // Create client options for stdio transport
                var clientOptions = new McpClientOptions
                {
                    ClientInfo = new Implementation
                    {
                        Name = "SkillsQuickstart",
                        Version = "1.0.0"
                    }
                };

                // Build the stdio transport configuration
                var transportConfig = new StdioClientTransportOptions
                {
                    Command = serverConfig.Command,
                    Arguments = serverConfig.Arguments,
                    Name = serverConfig.Name
                };

                // Add environment variables if specified
                if (serverConfig.Environment.Count > 0)
                {
                    transportConfig.EnvironmentVariables = new Dictionary<string, string?>(
                        serverConfig.Environment.Select(kvp =>
                            new KeyValuePair<string, string?>(kvp.Key, kvp.Value)));
                }

                // Create and connect the client
                var client = await McpClientFactory.CreateAsync(
                    new StdioClientTransport(transportConfig),
                    clientOptions);

                _clients[serverConfig.Name] = client;

                // Discover and register tools from this server
                var tools = await client.ListToolsAsync();
                foreach (var tool in tools)
                {
                    _toolRegistry[tool.Name] = (serverConfig.Name, tool);
                    Console.WriteLine($"    Registered tool: {tool.Name}");
                }

                Console.WriteLine($"  Connected to {serverConfig.Name} with {tools.Count} tools");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Failed to connect to {serverConfig.Name}: {ex.Message}");
            }
        }

        _initialized = true;
    }

    /// <summary>
    /// Gets all available tools from connected MCP servers as OpenAI chat tools.
    /// </summary>
    public IReadOnlyList<ChatTool> GetAvailableTools()
    {
        var chatTools = new List<ChatTool>();

        foreach (var (toolName, (_, tool)) in _toolRegistry)
        {
            // Convert MCP tool JsonElement schema to BinaryData for OpenAI
            var schemaJson = tool.JsonSchema.ValueKind != JsonValueKind.Undefined
                ? tool.JsonSchema.GetRawText()
                : "{}";
            var parameters = BinaryData.FromString(schemaJson);

            var chatTool = ChatTool.CreateFunctionTool(
                functionName: toolName,
                functionDescription: tool.Description ?? string.Empty,
                functionParameters: parameters);

            chatTools.Add(chatTool);
        }

        return chatTools;
    }

    /// <summary>
    /// Executes a tool call and returns the result.
    /// </summary>
    public async Task<string> ExecuteToolAsync(
        string toolName,
        string argumentsJson,
        CancellationToken cancellationToken = default)
    {
        if (!_toolRegistry.TryGetValue(toolName, out var entry))
        {
            return $"Error: Tool '{toolName}' not found in any connected MCP server.";
        }

        var (serverName, mcpTool) = entry;

        if (!_clients.TryGetValue(serverName, out var client))
        {
            return $"Error: MCP server '{serverName}' not connected.";
        }

        try
        {
            // Parse arguments JSON to dictionary
            var arguments = string.IsNullOrEmpty(argumentsJson)
                ? new Dictionary<string, object?>()
                : JsonSerializer.Deserialize<Dictionary<string, object?>>(argumentsJson)
                  ?? new Dictionary<string, object?>();

            // Execute the tool call using the McpClientTool directly
            var result = await mcpTool.CallAsync(arguments);

            // Extract text content from the result
            var textParts = result.Content
                .Where(c => c.Type == "text")
                .Select(c => c.Text)
                .Where(t => t != null);

            return string.Join("\n", textParts);
        }
        catch (Exception ex)
        {
            return $"Error executing tool '{toolName}': {ex.Message}";
        }
    }

    /// <summary>
    /// Gets the names of all connected servers.
    /// </summary>
    public IReadOnlyList<string> GetConnectedServerNames()
    {
        return _clients.Keys.ToList();
    }

    /// <summary>
    /// Disposes all MCP client connections.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        foreach (var client in _clients.Values)
        {
            if (client is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (client is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _clients.Clear();
        _toolRegistry.Clear();
    }
}
