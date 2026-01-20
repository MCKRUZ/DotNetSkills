using OpenAI.Chat;

namespace SkillsQuickstart.Services;

/// <summary>
/// Service for managing MCP server connections and executing tool calls.
/// </summary>
public interface IMcpClientService : IAsyncDisposable
{
    /// <summary>
    /// Initializes connections to all configured MCP servers.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available tools from connected MCP servers as OpenAI chat tools.
    /// </summary>
    IReadOnlyList<ChatTool> GetAvailableTools();

    /// <summary>
    /// Executes a tool call and returns the result.
    /// </summary>
    Task<string> ExecuteToolAsync(string toolName, string argumentsJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the names of all connected servers.
    /// </summary>
    IReadOnlyList<string> GetConnectedServerNames();
}
