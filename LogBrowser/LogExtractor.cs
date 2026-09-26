using System.Text.RegularExpressions;

namespace LogBrowser;

internal static partial class LogExtractor
{
    private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

    // Matches Serilog static calls (Log.Error(...)) and ILogger extensions (_logger.LogError(...)),
    // then captures the first string literal among the arguments, possibly spanning several lines.
    [GeneratedRegex("""
        (?:\bLog\.|\.Log)(?<level>Trace|Verbose|Debug|Information|Warning|Error|Critical|Fatal)\s*\(
        [^;"]*?
        (?:\$@|@\$|\$|@)?"(?<message>(?:[^"\\\r\n]|\\.)*)"
        """, RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex LogCallRegex();

    public static IEnumerable<Log> Extract(string sourceCode)
    {
        foreach (Match match in LogCallRegex().Matches(sourceCode))
        {
            var message = match.Groups["message"].Value;
            if (string.IsNullOrWhiteSpace(message)) continue;

            yield return new Log(Enum.Parse<LogLevel>(match.Groups["level"].Value), message);
        }
    }

    public static IEnumerable<string> GetSourceFiles(string directory)
    {
        var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };

        return Directory.EnumerateFiles(directory, "*.cs", options)
            .Where(path => !Path.GetRelativePath(directory, path)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(ExcludedDirectories.Contains));
    }

    /// <summary>
    /// Returns the distinct logs of a directory, most severe level first, messages sorted alphabetically.
    /// </summary>
    public static IReadOnlyList<Log> ExtractFromDirectory(string directory) =>
        GetSourceFiles(directory)
            .SelectMany(path => Extract(File.ReadAllText(path)))
            .Distinct()
            .OrderByDescending(log => log.Level)
            .ThenBy(log => log.Message, StringComparer.Ordinal)
            .ToList();
}
