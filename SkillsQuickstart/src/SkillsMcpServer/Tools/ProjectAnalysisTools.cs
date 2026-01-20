using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace SkillsMcpServer.Tools;

/// <summary>
/// MCP tools for analyzing project structure and code.
/// </summary>
[McpServerToolType]
public static class ProjectAnalysisTools
{
    /// <summary>
    /// Analyzes directory structure and returns a tree view.
    /// </summary>
    [McpServerTool(Name = "analyze_directory"), Description("Analyzes a directory and returns its structure as a tree. Shows files and folders with their sizes.")]
    public static string AnalyzeDirectory(
        [Description("The directory path to analyze")] string path,
        [Description("Maximum depth to traverse (default: 3)")] int maxDepth = 3,
        [Description("File extensions to include (comma-separated, e.g., '.cs,.json'). Empty means all files.")] string? extensions = null)
    {
        if (!Directory.Exists(path))
        {
            return $"Error: Directory '{path}' does not exist.";
        }

        var extensionFilter = string.IsNullOrEmpty(extensions)
            ? null
            : extensions.Split(',').Select(e => e.Trim().ToLowerInvariant()).ToHashSet();

        var output = new StringBuilder();
        output.AppendLine($"# Directory Analysis: {path}");
        output.AppendLine();

        var stats = new DirectoryStats();
        BuildTree(path, output, "", true, 0, maxDepth, extensionFilter, stats);

        output.AppendLine();
        output.AppendLine("## Summary");
        output.AppendLine($"- Total Directories: {stats.DirectoryCount}");
        output.AppendLine($"- Total Files: {stats.FileCount}");
        output.AppendLine($"- Total Size: {FormatSize(stats.TotalSize)}");

        return output.ToString();
    }

    /// <summary>
    /// Counts lines of code in a directory by file type.
    /// </summary>
    [McpServerTool(Name = "count_lines"), Description("Counts lines of code in a directory, grouped by file extension. Returns total lines, code lines, and blank lines.")]
    public static string CountLines(
        [Description("The directory path to analyze")] string path,
        [Description("File extensions to include (comma-separated, e.g., '.cs,.ts,.js'). Required.")] string extensions)
    {
        if (!Directory.Exists(path))
        {
            return $"Error: Directory '{path}' does not exist.";
        }

        var extensionList = extensions.Split(',')
            .Select(e => e.Trim().ToLowerInvariant())
            .Where(e => !string.IsNullOrEmpty(e))
            .ToList();

        if (extensionList.Count == 0)
        {
            return "Error: At least one file extension must be specified.";
        }

        var results = new Dictionary<string, LineCount>();

        foreach (var ext in extensionList)
        {
            results[ext] = new LineCount();
        }

        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (!results.ContainsKey(ext)) continue;

            try
            {
                var lines = File.ReadAllLines(file);
                results[ext].FileCount++;
                results[ext].TotalLines += lines.Length;
                results[ext].BlankLines += lines.Count(l => string.IsNullOrWhiteSpace(l));
                results[ext].CodeLines += lines.Count(l => !string.IsNullOrWhiteSpace(l));
            }
            catch
            {
                // Skip files that can't be read
            }
        }

        var output = new StringBuilder();
        output.AppendLine($"# Lines of Code Analysis: {path}");
        output.AppendLine();
        output.AppendLine("| Extension | Files | Total Lines | Code Lines | Blank Lines |");
        output.AppendLine("|-----------|-------|-------------|------------|-------------|");

        var grandTotal = new LineCount();
        foreach (var (ext, count) in results.OrderByDescending(r => r.Value.CodeLines))
        {
            output.AppendLine($"| {ext} | {count.FileCount} | {count.TotalLines:N0} | {count.CodeLines:N0} | {count.BlankLines:N0} |");
            grandTotal.FileCount += count.FileCount;
            grandTotal.TotalLines += count.TotalLines;
            grandTotal.CodeLines += count.CodeLines;
            grandTotal.BlankLines += count.BlankLines;
        }

        output.AppendLine($"| **Total** | **{grandTotal.FileCount}** | **{grandTotal.TotalLines:N0}** | **{grandTotal.CodeLines:N0}** | **{grandTotal.BlankLines:N0}** |");

        return output.ToString();
    }

    /// <summary>
    /// Finds patterns like TODO, FIXME, HACK in code files.
    /// </summary>
    [McpServerTool(Name = "find_patterns"), Description("Searches for patterns (like TODO, FIXME, HACK) in code files. Returns matching lines with file locations.")]
    public static string FindPatterns(
        [Description("The directory path to search")] string path,
        [Description("Patterns to search for (comma-separated, e.g., 'TODO,FIXME,HACK')")] string patterns,
        [Description("File extensions to search (comma-separated, e.g., '.cs,.ts'). Required.")] string extensions)
    {
        if (!Directory.Exists(path))
        {
            return $"Error: Directory '{path}' does not exist.";
        }

        var patternList = patterns.Split(',')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();

        var extensionList = extensions.Split(',')
            .Select(e => e.Trim().ToLowerInvariant())
            .Where(e => !string.IsNullOrEmpty(e))
            .ToHashSet();

        if (patternList.Count == 0)
        {
            return "Error: At least one pattern must be specified.";
        }

        var output = new StringBuilder();
        output.AppendLine($"# Pattern Search: {path}");
        output.AppendLine($"Searching for: {string.Join(", ", patternList)}");
        output.AppendLine();

        var findings = new List<(string Pattern, string File, int Line, string Text)>();
        var regex = new Regex(
            $@"\b({string.Join("|", patternList.Select(Regex.Escape))})\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (!extensionList.Contains(ext)) continue;

            try
            {
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    var match = regex.Match(lines[i]);
                    if (match.Success)
                    {
                        var relativePath = Path.GetRelativePath(path, file);
                        findings.Add((match.Value.ToUpper(), relativePath, i + 1, lines[i].Trim()));
                    }
                }
            }
            catch
            {
                // Skip files that can't be read
            }
        }

        if (findings.Count == 0)
        {
            output.AppendLine("No patterns found.");
            return output.ToString();
        }

        // Group by pattern
        foreach (var group in findings.GroupBy(f => f.Pattern).OrderBy(g => g.Key))
        {
            output.AppendLine($"## {group.Key} ({group.Count()})");
            output.AppendLine();

            foreach (var (_, file, line, text) in group.Take(20)) // Limit to 20 per pattern
            {
                output.AppendLine($"- **{file}:{line}**");
                output.AppendLine($"  `{(text.Length > 100 ? text[..100] + "..." : text)}`");
            }

            if (group.Count() > 20)
            {
                output.AppendLine($"  ... and {group.Count() - 20} more");
            }

            output.AppendLine();
        }

        output.AppendLine($"## Summary");
        output.AppendLine($"Total findings: {findings.Count}");

        return output.ToString();
    }

    private static void BuildTree(
        string path,
        StringBuilder output,
        string indent,
        bool isLast,
        int depth,
        int maxDepth,
        HashSet<string>? extensionFilter,
        DirectoryStats stats)
    {
        var dirInfo = new DirectoryInfo(path);
        var prefix = isLast ? "└── " : "├── ";
        var name = depth == 0 ? dirInfo.FullName : dirInfo.Name;

        output.AppendLine($"{indent}{prefix}{name}/");
        stats.DirectoryCount++;

        if (depth >= maxDepth)
        {
            output.AppendLine($"{indent}    └── ...");
            return;
        }

        var newIndent = indent + (isLast ? "    " : "│   ");

        // Get files
        var files = dirInfo.GetFiles()
            .Where(f => extensionFilter == null || extensionFilter.Contains(f.Extension.ToLowerInvariant()))
            .OrderBy(f => f.Name)
            .ToList();

        // Get directories (exclude common non-essential folders)
        var dirs = dirInfo.GetDirectories()
            .Where(d => !d.Name.StartsWith(".") &&
                       d.Name != "node_modules" &&
                       d.Name != "bin" &&
                       d.Name != "obj" &&
                       d.Name != "packages")
            .OrderBy(d => d.Name)
            .ToList();

        var items = files.Count + dirs.Count;
        var index = 0;

        foreach (var file in files)
        {
            index++;
            var filePrefix = index == items ? "└── " : "├── ";
            output.AppendLine($"{newIndent}{filePrefix}{file.Name} ({FormatSize(file.Length)})");
            stats.FileCount++;
            stats.TotalSize += file.Length;
        }

        foreach (var dir in dirs)
        {
            index++;
            BuildTree(dir.FullName, output, newIndent, index == items, depth + 1, maxDepth, extensionFilter, stats);
        }
    }

    private static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int i = 0;
        double size = bytes;
        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:0.##} {suffixes[i]}";
    }

    private class DirectoryStats
    {
        public int DirectoryCount { get; set; }
        public int FileCount { get; set; }
        public long TotalSize { get; set; }
    }

    private class LineCount
    {
        public int FileCount { get; set; }
        public int TotalLines { get; set; }
        public int CodeLines { get; set; }
        public int BlankLines { get; set; }
    }
}
