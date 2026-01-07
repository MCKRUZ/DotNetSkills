using SkillsQuickstart.Models;

namespace SkillsQuickstart.Services;

/// <summary>
/// Interface for executing skills with AI models.
/// </summary>
public interface ISkillExecutor
{
    /// <summary>
    /// Executes a skill with the given user input.
    /// </summary>
    /// <param name="skill">The skill to execute.</param>
    /// <param name="userInput">The user input to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The AI response.</returns>
    Task<string> ExecuteAsync(SkillDefinition skill, string userInput, CancellationToken ct = default);

    /// <summary>
    /// Checks if the executor is properly configured and ready to use.
    /// </summary>
    bool IsConfigured { get; }
}
