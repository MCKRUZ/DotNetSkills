namespace SkillsQuickstart.Config;

/// <summary>
/// Configuration for selecting the LLM provider (Azure or OpenAI).
/// </summary>
public class LlmProviderConfig
{
    public const string SectionName = "LlmProvider";

    /// <summary>
    /// The LLM provider to use: "azure" or "openai".
    /// </summary>
    public string Provider { get; set; } = "azure";
}
