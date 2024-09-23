# Readme

## LogBrowser 

Before using it, please make sure your lauchsettings.json is correctly configured with the right commandLineArgs.

The first argument is the folder to analyse. 
The second is the output markdown file. 

Example

```json
{
  "profiles": {
    "LogBrowser": {
      "commandName": "Project",
      "commandLineArgs": "C:\\Users\\xxx\\source\\repos\\projetX\\src\\ logs.md"
    }
  }
}
```

Then run the project. A console app will be shown with the found logs. Press any key to close the app when asked. 
You will find the result file in your specified output. 

File content example

### Log list

- **Critical**

	- `Error when getting access token in AuthorizationToken`

- **Error**

	- `Unable to deserialize request result when calling API`
	- `Unable to process request result when calling API`
	- `The given tenant {tenant} is unknown`
	- `Error when calling API : status code: {response.StatusCode}, body: {responseContent}`
	- `Exception Handled`