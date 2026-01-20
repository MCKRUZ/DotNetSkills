using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using ModelContextProtocol.Server;

namespace SkillsMcpServer.Tools;

/// <summary>
/// MCP tools for interacting with GitHub via the gh CLI.
/// Requires: GitHub CLI (gh) installed and authenticated.
/// </summary>
[McpServerToolType]
public static class GitHubTools
{
    /// <summary>
    /// Lists issues from a GitHub repository.
    /// </summary>
    [McpServerTool(Name = "github_list_issues"), Description("Lists issues from a GitHub repository. Requires gh CLI to be installed and authenticated.")]
    public static async Task<string> ListIssues(
        [Description("Repository in format 'owner/repo' (e.g., 'microsoft/vscode')")] string repository,
        [Description("Filter by state: open, closed, or all (default: open)")] string state = "open",
        [Description("Maximum number of issues to return (default: 10)")] int limit = 10,
        [Description("Filter by labels (comma-separated)")] string? labels = null)
    {
        var args = new StringBuilder($"issue list --repo {repository} --state {state} --limit {limit}");

        if (!string.IsNullOrEmpty(labels))
        {
            args.Append($" --label \"{labels}\"");
        }

        args.Append(" --json number,title,state,author,labels,createdAt,url");

        var result = await RunGhCommand(args.ToString());

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        return $"# Issues for {repository}\n\n{FormatIssuesJson(result)}";
    }

    /// <summary>
    /// Gets details of a specific GitHub issue.
    /// </summary>
    [McpServerTool(Name = "github_get_issue"), Description("Gets detailed information about a specific GitHub issue including comments.")]
    public static async Task<string> GetIssue(
        [Description("Repository in format 'owner/repo'")] string repository,
        [Description("Issue number")] int issueNumber)
    {
        var result = await RunGhCommand(
            $"issue view {issueNumber} --repo {repository} --json number,title,state,body,author,labels,comments,createdAt,url");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        return $"# Issue #{issueNumber} in {repository}\n\n{result}";
    }

    /// <summary>
    /// Searches GitHub repositories.
    /// </summary>
    [McpServerTool(Name = "github_search_repos"), Description("Searches for GitHub repositories matching a query.")]
    public static async Task<string> SearchRepos(
        [Description("Search query (e.g., 'dotnet mcp language:csharp')")] string query,
        [Description("Maximum number of results (default: 10)")] int limit = 10)
    {
        var result = await RunGhCommand(
            $"search repos \"{query}\" --limit {limit} --json fullName,description,stargazerCount,primaryLanguage,url");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        return $"# Repository Search: {query}\n\n{result}";
    }

    /// <summary>
    /// Lists pull requests from a GitHub repository.
    /// </summary>
    [McpServerTool(Name = "github_list_prs"), Description("Lists pull requests from a GitHub repository.")]
    public static async Task<string> ListPullRequests(
        [Description("Repository in format 'owner/repo'")] string repository,
        [Description("Filter by state: open, closed, merged, or all (default: open)")] string state = "open",
        [Description("Maximum number of PRs to return (default: 10)")] int limit = 10)
    {
        var result = await RunGhCommand(
            $"pr list --repo {repository} --state {state} --limit {limit} --json number,title,state,author,createdAt,url");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        return $"# Pull Requests for {repository}\n\n{result}";
    }

    /// <summary>
    /// Gets repository information.
    /// </summary>
    [McpServerTool(Name = "github_repo_info"), Description("Gets detailed information about a GitHub repository.")]
    public static async Task<string> GetRepoInfo(
        [Description("Repository in format 'owner/repo'")] string repository)
    {
        var result = await RunGhCommand(
            $"repo view {repository} --json name,description,stargazerCount,forkCount,primaryLanguage,licenseInfo,createdAt,pushedAt,url");

        if (result.StartsWith("Error:"))
        {
            return result;
        }

        return $"# Repository: {repository}\n\n{result}";
    }

    private static async Task<string> RunGhCommand(string arguments, int timeoutSeconds = 30)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "gh",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,  // Prevent waiting for input
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            process.Start();
            process.StandardInput.Close();  // Close stdin immediately

            // Read stdout and stderr concurrently to avoid deadlocks
            var outputTask = process.StandardOutput.ReadToEndAsync(cts.Token);
            var errorTask = process.StandardError.ReadToEndAsync(cts.Token);

            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return $"Error: Command timed out after {timeoutSeconds} seconds.";
            }

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                if (error.Contains("gh auth login"))
                {
                    return "Error: GitHub CLI is not authenticated. Run 'gh auth login' first.";
                }
                if (error.Contains("not found") || error.Contains("Could not resolve"))
                {
                    return $"Error: Repository not found or you don't have access.";
                }
                return $"Error: {error}";
            }

            return string.IsNullOrEmpty(output) ? "No results found." : output;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return "Error: GitHub CLI (gh) is not installed. Install it from https://cli.github.com/";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    private static string FormatIssuesJson(string json)
    {
        // Just return the raw JSON for now - the LLM can interpret it
        // In a production system, you might want to format this more nicely
        return json;
    }
}
