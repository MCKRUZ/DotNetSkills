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
/// Configuration for a single MCP server.
/// </summary>
public class McpServerEntry
{
    /// <summary>
    /// Unique name for this server (used for identification).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Path to the executable or command to run.
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Arguments to pass to the command.
    /// </summary>
    public List<string> Arguments { get; set; } = new();

    /// <summary>
    /// Environment variables to set for the server process.
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new();

    /// <summary>
    /// Whether this server is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
