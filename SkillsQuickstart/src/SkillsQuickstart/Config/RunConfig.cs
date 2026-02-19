namespace SkillsQuickstart.Config;

/// <summary>
/// Optional pre-defined run configuration. When set, the program skips interactive prompts
/// and uses these values directly (headless/automated execution mode).
/// </summary>
public class RunConfig
{
    public const string SectionName = "RunConfig";

    /// <summary>
    /// The skill ID to execute. If set, skips the interactive skill selection prompt.
    /// </summary>
    public string? SkillId { get; set; }

    /// <summary>
    /// The user request/input to pass to the skill. If set, skips the interactive text prompt.
    /// </summary>
    public string? UserInput { get; set; }

    /// <summary>
    /// If true, exits after one run without prompting "Run another skill?".
    /// Defaults to true when SkillId and UserInput are both set.
    /// </summary>
    public bool? RunOnce { get; set; }

    /// <summary>
    /// Returns true if all required inputs are pre-configured for headless execution.
    /// </summary>
    public bool IsHeadless => !string.IsNullOrWhiteSpace(SkillId) && !string.IsNullOrWhiteSpace(UserInput);

    /// <summary>
    /// Returns true if the run should exit after a single execution.
    /// </summary>
    public bool ShouldRunOnce => RunOnce ?? IsHeadless;
}
