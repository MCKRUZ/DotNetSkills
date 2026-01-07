namespace SkillsQuickstart.Config;

/// <summary>
/// Configuration for Azure OpenAI service.
/// Bound from appsettings.json "AzureOpenAI" section.
/// </summary>
public class AzureOpenAIConfig
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "AzureOpenAI";

    /// <summary>
    /// Azure OpenAI endpoint (e.g., https://your-resource-name.openai.azure.com/).
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Deployment name for the model (e.g., gpt-4).
    /// </summary>
    public string DeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// API version for Azure OpenAI.
    /// </summary>
    public string ApiVersion { get; set; } = "2024-02-15-preview";

    /// <summary>
    /// Model name (e.g., gpt-4, gpt-35-turbo).
    /// </summary>
    public string ModelName { get; set; } = "gpt-4";

    /// <summary>
    /// Maximum tokens for completion.
    /// </summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// Temperature for response generation (0.0 to 2.0).
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Validates that required configuration is present.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Endpoint)
            && !string.IsNullOrWhiteSpace(ApiKey)
            && !string.IsNullOrWhiteSpace(DeploymentName);
    }
}
