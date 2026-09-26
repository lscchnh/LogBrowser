namespace LogBrowser;

/// <summary>
/// Log levels from both Microsoft.Extensions.Logging and Serilog, ordered by severity.
/// </summary>
public enum LogLevel
{
    Trace,
    Verbose,
    Debug,
    Information,
    Warning,
    Error,
    Critical,
    Fatal
}

public sealed record Log(LogLevel Level, string Message);
