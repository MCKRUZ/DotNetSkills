using OpenAI.Chat;

namespace SkillsQuickstart.Services;

/// <summary>
/// Represents the result of a chat completion.
/// </summary>
public class ChatCompletionResult
{
    /// <summary>
    /// The text response (if no tool calls).
    /// </summary>
    public string? TextResponse { get; init; }

    /// <summary>
    /// Tool calls requested by the model.
    /// </summary>
    public IReadOnlyList<ChatToolCall> ToolCalls { get; init; } = Array.Empty<ChatToolCall>();

    /// <summary>
    /// Whether the model requested tool calls.
    /// </summary>
    public bool HasToolCalls => ToolCalls.Count > 0;

    /// <summary>
    /// The finish reason from the API.
    /// </summary>
    public ChatFinishReason? FinishReason { get; init; }
}

/// <summary>
/// Service for interacting with Azure OpenAI.
/// </summary>
public interface IAzureOpenAIService
{
    /// <summary>
    /// Sends a chat completion request with optional tools.
    /// </summary>
    Task<ChatCompletionResult> GetCompletionAsync(
        IEnumerable<ChatMessage> messages,
        IEnumerable<ChatTool>? tools = null,
        CancellationToken cancellationToken = default);
}
