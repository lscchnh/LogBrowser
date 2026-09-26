using System.Text;

namespace LogBrowser;

internal static class MarkdownExporter
{
    /// <summary>
    /// Renders logs grouped by level, keeping the order of <paramref name="logs"/>.
    /// </summary>
    public static string Render(IEnumerable<Log> logs)
    {
        var builder = new StringBuilder();
        builder.AppendLine("### Log list");

        foreach (var group in logs.GroupBy(log => log.Level))
        {
            builder.AppendLine();
            builder.AppendLine($"- **{group.Key}**");
            builder.AppendLine();

            foreach (var log in group)
            {
                builder.AppendLine($"\t- {ToInlineCode(log.Message)}");
            }
        }

        return builder.ToString();
    }

    // A message containing backticks needs a longer delimiter to stay a single code span.
    private static string ToInlineCode(string text) =>
        text.Contains('`') ? $"`` {text} ``" : $"`{text}`";
}
