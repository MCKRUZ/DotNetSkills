namespace SkillsQuickstart.Config;

/// <summary>
/// Configuration for MCP server connections.
/// </summary>
public class McpServersConfig
{
    public const string SectionName = "McpServers";

    /// <summary>
    /// List of MCP servers to connect to.
    /// </summary>
    public List<McpServerEntry> Servers { get; set; } = new();
}

/// <summary>
/// Transport type for MCP server connections.
/// </summary>
public enum McpTransportType
{
    /// <summary>
    /// Standard I/O transport (spawns a process).
    /// </summary>
    Stdio,

    /// <summary>
    /// HTTP transport (connects to a remote endpoint).
    /// </summary>
    Http
}

/// <summary>
/// Configuration for a single MCP server.
/// </summary>
public class McpServerEntry
{
    /// <summary>
    /// Unique name for this server (used for identification).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Transport type: "Stdio" (default) or "Http".
    /// </summary>
    public McpTransportType Type { get; set; } = McpTransportType.Stdio;

    /// <summary>
    /// Path to the executable or command to run (for Stdio transport).
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Arguments to pass to the command (for Stdio transport).
    /// </summary>
    public List<string> Arguments { get; set; } = new();

    /// <summary>
    /// Environment variables to set for the server process (for Stdio transport).
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new();

    /// <summary>
    /// HTTP endpoint URL (for Http transport).
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Whether this server is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
