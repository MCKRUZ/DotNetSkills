using Microsoft.Extensions.Options;
using SkillsQuickstart.Config;
using SkillsQuickstart.Models;
using System.Text;

namespace SkillsQuickstart.Services;

/// <summary>
/// Implementation of skill execution using Azure OpenAI.
/// </summary>
public class AzureOpenAISkillExecutor : ISkillExecutor
{
    private readonly AzureOpenAIConfig _config;
    private readonly ISkillLoader _skillLoader;

    public AzureOpenAISkillExecutor(
        IOptions<AzureOpenAIConfig> config,
        ISkillLoader skillLoader)
    {
        _config = config.Value;
        _skillLoader = skillLoader;
    }

    /// <inheritdoc />
    public bool IsConfigured => _config.IsValid();

    /// <inheritdoc />
    public async Task<string> ExecuteAsync(SkillDefinition skill, string userInput, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "Azure OpenAI is not configured. Please set the Endpoint, ApiKey, and DeploymentName in appsettings.json");
        }

        // Ensure skill is fully loaded with instructions
        if (!skill.IsFullyLoaded)
        {
            skill = await _skillLoader.LoadSkillAsync(skill.Id, ct)
                ?? throw new InvalidOperationException($"Failed to load skill: {skill.Id}");
        }

        // Build the system prompt with skill instructions
        var systemPrompt = BuildSystemPrompt(skill);

        // For demonstration purposes, we'll create a mock response
        // In production, you would call Azure OpenAI API here
        try
        {
            Console.WriteLine($"Azure OpenAI Configuration:");
            Console.WriteLine($"  Endpoint:    {_config.Endpoint}");
            Console.WriteLine($"  Deployment:  {_config.DeploymentName}");
            Console.WriteLine($"  Model:       {_config.ModelName}");
            Console.WriteLine($"  Max Tokens:  {_config.MaxTokens}");
            Console.WriteLine($"  Temperature: {_config.Temperature}");
            Console.WriteLine();

            Console.WriteLine("System Prompt (Skill Instructions):");
            Console.WriteLine("────────────────────────────────────");
            Console.WriteLine(systemPrompt.Length > 500 ? systemPrompt[..500] + "..." : systemPrompt);
            Console.WriteLine("────────────────────────────────────");
            Console.WriteLine();

            Console.WriteLine($"User Input: {userInput}");
            Console.WriteLine();

            // NOTE: This is a placeholder implementation
            // In production, you would use Azure OpenAI SDK 2.x API which requires:
            // - Azure.AI.OpenAI 2.0.0+
            // - System.ClientModel.Primitives for ApiKeyCredential
            // - New message types (SystemChatMessage, UserChatMessage, etc.)
            // The actual implementation would look like:
            //
            // var client = new AzureOpenAIClient(new Uri(_config.Endpoint), new ApiKeyCredential(_config.ApiKey));
            // var chatClient = client.GetChatClient(_config.DeploymentName);
            // var messages = new List<ChatMessage>
            // {
            //     new SystemChatMessage(systemPrompt),
            //     new UserChatMessage(userInput)
            // };
            // var options = new ChatCompletionOptions { MaxTokens = _config.MaxTokens, Temperature = (float)_config.Temperature };
            // ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options, ct);
            // return completion.Content[0].Text ?? string.Empty;

            Console.WriteLine("To enable actual Azure OpenAI execution:");
            Console.WriteLine("1. Ensure you have valid Azure OpenAI credentials in appsettings.json");
            Console.WriteLine("2. Implement the actual API call using Azure.AI.OpenAI 2.x SDK");
            Console.WriteLine("3. Uncomment the execution code in this service");
            Console.WriteLine();

            return $"[MOCK RESPONSE] This is where the Azure OpenAI response would appear. Your skill '{skill.Name}' processed input: '{userInput}'";
        }
        catch (Exception ex)
        {
            return $"Error executing skill: {ex.Message}";
        }
    }

    /// <summary>
    /// Builds a system prompt from the skill definition.
    /// </summary>
    private string BuildSystemPrompt(SkillDefinition skill)
    {
        var prompt = new StringBuilder();

        // Add skill instructions
        if (!string.IsNullOrWhiteSpace(skill.Instructions))
        {
            prompt.AppendLine(skill.Instructions);
            prompt.AppendLine();
        }

        // Add information about available resources
        if (skill.HasTemplates || skill.HasReferences)
        {
            prompt.AppendLine("Available Resources:");
            prompt.AppendLine(new string('=', 50));

            if (skill.HasTemplates)
            {
                prompt.AppendLine("\nTemplates:");
                foreach (var template in skill.Templates)
                {
                    prompt.AppendLine($"  - {template.RelativePath}");
                }
            }

            if (skill.HasReferences)
            {
                prompt.AppendLine("\nReferences:");
                foreach (var reference in skill.References)
                {
                    prompt.AppendLine($"  - {reference.RelativePath}");
                }
            }

            prompt.AppendLine();
            prompt.AppendLine("Note: You can request to use templates or reference materials as needed.");
            prompt.AppendLine();
        }

        return prompt.ToString();
    }
}
