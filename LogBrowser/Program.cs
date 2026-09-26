using LogBrowser;

const string DefaultOutputFile = "logs.md";
const string Banner = @"
  _                 ____
 | |               |  _ \
 | |     ___   __ _| |_) |_ __ _____      _____  ___ _ __
 | |    / _ \ / _` |  _ <| '__/ _ \ \ /\ / / __|/ _ \ '__|
 | |___| (_) | (_| | |_) | | | (_) \ V  V /\__ \  __/ |
 |______\___/ \__, |____/|_|  \___/ \_/\_/ |___/\___|_|
               __/ |
              |___/";

if (args.Length is 0 or > 2 || string.IsNullOrWhiteSpace(args[0]))
{
    Console.Error.WriteLine($"Usage: LogBrowser <directory-to-scan> [output-file (default: {DefaultOutputFile})]");
    return 1;
}

var directoryToScan = Path.GetFullPath(args[0]);
var outputFile = Path.GetFullPath(args.Length == 2 ? args[1] : DefaultOutputFile);

if (!Directory.Exists(directoryToScan))
{
    Console.Error.WriteLine($"Directory not found: {directoryToScan}");
    return 1;
}

Console.WriteLine(Banner);
Console.WriteLine($"Directory to scan : {directoryToScan}");
Console.WriteLine($"Output file       : {outputFile}");
Console.WriteLine();

try
{
    var logs = LogExtractor.ExtractFromDirectory(directoryToScan);

    if (logs.Count == 0)
    {
        Console.WriteLine("No logs were found in this directory.");
        return 2;
    }

    foreach (var log in logs)
    {
        Console.WriteLine($"{log.Level,-12} {log.Message}");
    }

    File.WriteAllText(outputFile, MarkdownExporter.Render(logs));

    Console.WriteLine();
    Console.WriteLine($"{logs.Count} log(s) exported to {outputFile}");
    return 0;
}
catch (Exception e) when (e is IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Error while processing the scan: {e.Message}");
    return 1;
}
