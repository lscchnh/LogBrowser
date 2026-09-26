# LogBrowser

LogBrowser scans a folder of C# source files, extracts every log message and exports them as a Markdown list grouped by level.
It is handy to review the wording of your logs or to document them.

## Supported log calls

- Serilog static logger: `Log.Information("...")`, `Log.Error(ex, "...")`, ...
- `Microsoft.Extensions.Logging` extensions: `_logger.LogWarning("...")`, `logger.LogCritical(ex, "...")`, ...

Levels: `Trace`, `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `Fatal`.

The first string literal of the call is used as the message (regular, verbatim or interpolated), even when the call spans several lines.
Files under `bin` and `obj` folders are ignored, duplicates are removed, and levels are sorted from most to least severe.

## Usage

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project LogBrowser -- <directory-to-scan> [output-file]
```

- `directory-to-scan`: the folder to analyse (scanned recursively).
- `output-file`: the Markdown file to generate, `logs.md` by default. It is overwritten if it exists.

From Visual Studio, set the arguments in `LogBrowser/Properties/launchSettings.json`:

```json
{
  "profiles": {
    "LogBrowser": {
      "commandName": "Project",
      "commandLineArgs": "C:\\Users\\xxx\\source\\repos\\projetX\\src logs.md"
    }
  }
}
```

Exit codes: `0` success, `1` invalid arguments or I/O error, `2` no logs found.

## Output example

```markdown
### Log list

- **Critical**

	- `Error when getting access token in AuthorizationToken`

- **Error**

	- `Error when calling API : status code: {response.StatusCode}, body: {responseContent}`
	- `The given tenant {tenant} is unknown`
	- `Unable to deserialize request result when calling API`
```

## Tests

```bash
dotnet test
```
