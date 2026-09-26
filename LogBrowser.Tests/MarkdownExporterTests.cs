namespace LogBrowser.Tests;

public class MarkdownExporterTests
{
    [Fact]
    public void Render_GroupsLogsByLevel()
    {
        Log[] logs =
        [
            new(LogLevel.Critical, "Token failure"),
            new(LogLevel.Error, "Unable to deserialize"),
            new(LogLevel.Error, "Use `dotnet` instead"),
        ];

        var markdown = MarkdownExporter.Render(logs);

        var expected = string.Join(Environment.NewLine,
            "### Log list",
            "",
            "- **Critical**",
            "",
            "\t- `Token failure`",
            "",
            "- **Error**",
            "",
            "\t- `Unable to deserialize`",
            "\t- `` Use `dotnet` instead ``",
            "");
        Assert.Equal(expected, markdown);
    }
}
