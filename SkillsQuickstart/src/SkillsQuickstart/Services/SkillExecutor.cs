using OpenAI.Chat;
using SkillsCore.Models;

namespace SkillsQuickstart.Services;

/// <summary>
/// Orchestrates skill execution by coordinating between:
/// - Skill instructions (system prompt)
/// - Azure OpenAI (LLM for reasoning)
/// - MCP servers (tool execution)
///
/// Flow: User Input → LLM (with skill instructions) → Tool Calls → MCP → Results → LLM → Final Response
/// </summary>
public class SkillExecutor : ISkillExecutor
{
    private readonly IAzureOpenAIService _openAIService;
    private readonly IMcpClientService _mcpClientService;

    public SkillExecutor(IAzureOpenAIService openAIService, IMcpClientService mcpClientService)
    {
        _openAIService = openAIService;
        _mcpClientService = mcpClientService;
    }

    /// <summary>
    /// Executes a skill with the given user input.
    /// </summary>
    public async Task<SkillExecutionResult> ExecuteAsync(
        SkillDefinition skill,
        string userInput,
        int maxTurns = 10,
        CancellationToken cancellationToken = default)
    {
        var toolCallRecords = new List<ToolCallRecord>();
        var turnCount = 0;

        try
        {
            // Build the system prompt from skill instructions
            var systemPrompt = BuildSystemPrompt(skill);

            // Initialize conversation with system prompt and user input
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userInput)
            };

            // Get available tools from MCP servers
            var tools = _mcpClientService.GetAvailableTools();
            Console.WriteLine($"  Available tools: {tools.Count}");

            // Orchestration loop: call LLM, execute tools, repeat until done
            while (turnCount < maxTurns)
            {
                turnCount++;
                Console.WriteLine($"\n  Turn {turnCount}:");

                // Call Azure OpenAI
                var result = await _openAIService.GetCompletionAsync(messages, tools, cancellationToken);

                // If the model wants to call tools, execute them
                if (result.HasToolCalls)
                {
                    Console.WriteLine($"    LLM requested {result.ToolCalls.Count} tool call(s)");

                    // Add assistant message with tool calls to history
                    var assistantMessage = new AssistantChatMessage(result.ToolCalls);
                    messages.Add(assistantMessage);

                    // Execute each tool call
                    foreach (var toolCall in result.ToolCalls)
                    {
                        Console.WriteLine($"    Executing: {toolCall.FunctionName}");

                        var toolResult = await _mcpClientService.ExecuteToolAsync(
                            toolCall.FunctionName,
                            toolCall.FunctionArguments.ToString(),
                            cancellationToken);

                        // Record the tool call
                        toolCallRecords.Add(new ToolCallRecord
                        {
                            ToolName = toolCall.FunctionName,
                            Arguments = toolCall.FunctionArguments.ToString(),
                            Result = toolResult.Length > 200
                                ? toolResult[..200] + "..."
                                : toolResult
                        });

                        // Add tool result to conversation
                        var toolMessage = new ToolChatMessage(toolCall.Id, toolResult);
                        messages.Add(toolMessage);

                        Console.WriteLine($"    Result: {(toolResult.Length > 100 ? toolResult[..100] + "..." : toolResult)}");
                    }

                    // Continue the loop to let the LLM process tool results
                    continue;
                }

                // No tool calls - we have a final response
                if (result.FinishReason == ChatFinishReason.Stop)
                {
                    Console.WriteLine($"    LLM finished with response");

                    return new SkillExecutionResult
                    {
                        Response = result.TextResponse ?? string.Empty,
                        TurnCount = turnCount,
                        ToolCalls = toolCallRecords,
                        Success = true
                    };
                }

                // Unexpected finish reason
                Console.WriteLine($"    Unexpected finish reason: {result.FinishReason}");
                break;
            }

            // Max turns reached
            return new SkillExecutionResult
            {
                Response = "Execution stopped: maximum turns reached.",
                TurnCount = turnCount,
                ToolCalls = toolCallRecords,
                Success = false,
                Error = "Maximum turns exceeded"
            };
        }
        catch (Exception ex)
        {
            return new SkillExecutionResult
            {
                Response = string.Empty,
                TurnCount = turnCount,
                ToolCalls = toolCallRecords,
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Builds the system prompt from skill definition.
    /// </summary>
    private static string BuildSystemPrompt(SkillDefinition skill)
    {
        var prompt = new System.Text.StringBuilder();

        prompt.AppendLine($"# {skill.Name}");
        prompt.AppendLine();
        prompt.AppendLine(skill.Description);
        prompt.AppendLine();

        if (!string.IsNullOrEmpty(skill.Instructions))
        {
            prompt.AppendLine("## Instructions");
            prompt.AppendLine();
            prompt.AppendLine(skill.Instructions);
        }

        // Add context about available resources
        if (skill.TotalResourceCount > 0)
        {
            prompt.AppendLine();
            prompt.AppendLine("## Available Resources");
            prompt.AppendLine();
            prompt.AppendLine("The following resources are bundled with this skill:");

            foreach (var resource in skill.AllResources)
            {
                prompt.AppendLine($"- {resource.ResourceType}: {resource.RelativePath}");
            }
        }

        return prompt.ToString();
    }
}
