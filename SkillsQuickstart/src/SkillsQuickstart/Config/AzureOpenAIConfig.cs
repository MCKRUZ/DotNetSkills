namespace SkillsQuickstart.Config;

/// <summary>
/// Configuration for Azure OpenAI service connection.
/// </summary>
public class AzureOpenAIConfig
{
    public const string SectionName = "AzureOpenAI";

    /// <summary>
    /// The Azure OpenAI endpoint URL (e.g., https://your-resource.openai.azure.com/).
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// The Azure OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The deployment name for the model (e.g., gpt-4o, gpt-4-turbo).
    /// </summary>
    public string DeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// Maximum tokens in the response.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Temperature for response generation (0.0 to 2.0).
    /// </summary>
    public float Temperature { get; set; } = 0.7f;
}
