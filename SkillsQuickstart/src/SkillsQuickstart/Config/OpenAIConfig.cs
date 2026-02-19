namespace SkillsQuickstart.Config;

/// <summary>
/// Configuration for OpenAI service connection.
/// </summary>
public class OpenAIConfig
{
    public const string SectionName = "OpenAI";

    /// <summary>
    /// The OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The model name (e.g., gpt-4o, gpt-4-turbo, gpt-3.5-turbo).
    /// </summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>
    /// Maximum tokens in the response.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Temperature for response generation (0.0 - 2.0).
    /// </summary>
    public float Temperature { get; set; } = 0.7f;
}
