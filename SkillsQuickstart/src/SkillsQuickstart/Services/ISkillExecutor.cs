using SkillsCore.Models;

namespace SkillsQuickstart.Services;

/// <summary>
/// Result of a skill execution.
/// </summary>
public class SkillExecutionResult
{
    /// <summary>
    /// The final text response from the LLM.
    /// </summary>
    public string Response { get; init; } = string.Empty;

    /// <summary>
    /// Number of LLM turns taken.
    /// </summary>
    public int TurnCount { get; init; }

    /// <summary>
    /// Tool calls that were executed during this session.
    /// </summary>
    public IReadOnlyList<ToolCallRecord> ToolCalls { get; init; } = Array.Empty<ToolCallRecord>();

    /// <summary>
    /// Whether the execution completed successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if execution failed.
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// Record of a tool call during execution.
/// </summary>
public class ToolCallRecord
{
    public string ToolName { get; init; } = string.Empty;
    public string Arguments { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
}

/// <summary>
/// Service that orchestrates skill execution with LLM and MCP tools.
/// </summary>
public interface ISkillExecutor
{
    /// <summary>
    /// Executes a skill with the given user input.
    /// </summary>
    /// <param name="skill">The skill to execute.</param>
    /// <param name="userInput">The user's input/request.</param>
    /// <param name="maxTurns">Maximum number of LLM turns (to prevent infinite loops).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    Task<SkillExecutionResult> ExecuteAsync(
        SkillDefinition skill,
        string userInput,
        int maxTurns = 10,
        CancellationToken cancellationToken = default);
}
