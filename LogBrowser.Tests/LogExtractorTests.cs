namespace LogBrowser.Tests;

public class LogExtractorTests
{
    [Theory]
    [InlineData("""Log.Information("Started");""", LogLevel.Information, "Started")]
    [InlineData("""Log.Fatal("Crashed");""", LogLevel.Fatal, "Crashed")]
    [InlineData("""_logger.LogCritical("Token failure");""", LogLevel.Critical, "Token failure")]
    [InlineData("""_logger.LogError(ex, "Unable to process {Id}", id);""", LogLevel.Error, "Unable to process {Id}")]
    [InlineData("""Log.Warning($"Status {response.StatusCode}");""", LogLevel.Warning, "Status {response.StatusCode}")]
    [InlineData("""_logger.LogDebug(@"Verbatim");""", LogLevel.Debug, "Verbatim")]
    [InlineData("""Log.Error("Say \"hi\"");""", LogLevel.Error, "Say \\\"hi\\\"")]
    public void Extract_FindsLevelAndMessage(string code, LogLevel expectedLevel, string expectedMessage)
    {
        var log = Assert.Single(LogExtractor.Extract(code));

        Assert.Equal(new Log(expectedLevel, expectedMessage), log);
    }

    [Fact]
    public void Extract_HandlesMultilineCalls()
    {
        const string code = """
            _logger.LogWarning(
                exception,
                "Retrying {Attempt}",
                attempt);
            """;

        var log = Assert.Single(LogExtractor.Extract(code));

        Assert.Equal(new Log(LogLevel.Warning, "Retrying {Attempt}"), log);
    }

    [Theory]
    [InlineData("""Log.Error(exception.Message); var s = "not a log";""")]
    [InlineData("""Console.Error.WriteLine("not a log");""")]
    [InlineData("""catalog.Error("not a log");""")]
    [InlineData("""Log.Information("");""")]
    public void Extract_IgnoresNonLogStrings(string code)
    {
        Assert.Empty(LogExtractor.Extract(code));
    }

    [Fact]
    public void ExtractFromDirectory_DeduplicatesSortsAndSkipsBuildFolders()
    {
        var root = Directory.CreateTempSubdirectory("LogBrowserTests").FullName;
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "Cabinet"));
            Directory.CreateDirectory(Path.Combine(root, "bin"));
            File.WriteAllText(Path.Combine(root, "A.cs"), """
                Log.Information("Zeta");
                Log.Information("Alpha");
                Log.Error("Boom");
                """);
            File.WriteAllText(Path.Combine(root, "Cabinet", "B.cs"), """Log.Information("Alpha"); Log.Error("Alpha");""");
            File.WriteAllText(Path.Combine(root, "bin", "C.cs"), """Log.Fatal("Generated");""");

            var logs = LogExtractor.ExtractFromDirectory(root);

            Assert.Equal(
                [
                    new Log(LogLevel.Error, "Alpha"),
                    new Log(LogLevel.Error, "Boom"),
                    new Log(LogLevel.Information, "Alpha"),
                    new Log(LogLevel.Information, "Zeta"),
                ],
                logs);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
