using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogBrowser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (!InputsAreValid(args))
            {
                Console.WriteLine("Please fill launchsettings.json correctly. Refer to help.md if necessary.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            var repertoryToScan = args[0];
            var outputFilename = args[1];

            try
            {
                Console.Title = "LogBrowser";
                const string title = @"
  _                 ____                                  
 | |               |  _ \                                 
 | |     ___   __ _| |_) |_ __ _____      _____  ___ _ __ 
 | |    / _ \ / _` |  _ <| '__/ _ \ \ /\ / / __|/ _ \ '__|
 | |___| (_) | (_| | |_) | | | (_) \ V  V /\__ \  __/ |   
 |______\___/ \__, |____/|_|  \___/ \_/\_/ |___/\___|_|   
               __/ |                                      
              |___/   ";

                Console.WriteLine(title);
                Console.WriteLine($"Repertory to scan: {repertoryToScan}");
                Console.WriteLine($"Output file : {outputFilename}");
                Console.WriteLine("\r\nPress any key to start the scan.");
                Console.ReadLine();

                var filePaths = GetFilePathsToScan(repertoryToScan);

                DeleteFileIfExists(outputFilename);

                var logs = new List<Log>();
                foreach (var filePath in filePaths)
                {
                    using var myReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                    var line = " ";
                    while (line != null)
                    {
                        line = myReader.ReadLine();

                        if (line == null || !line.Contains("Log.")) continue;

                        Log log = new()
                        {
                            LogLevel = GetLogLevel(line),
                            LogContent = GetLogContent(line)
                        };

                        if(!string.IsNullOrWhiteSpace(log.LogContent) && !string.IsNullOrWhiteSpace(log.LogLevel))
                        {
                            logs.Add(log);
                        }                        
                    }
                }

                if (logs.Count == 0)
                {
                    Exit("No logs were found in this repertory or there was a problem during the scan.");
                }

                logs = logs.DistinctBy(x => x.LogContent).OrderBy(x => x.LogLevel).ToList();

                string outputFileFullPath = ExportInTextFile(logs, outputFilename);

                Exit($"\r\nLog file has been generated : {outputFileFullPath}");
            }
            catch (Exception e)
            {
                Exit($"\r\nError when processing the scan", e);
            }
        }

        private static bool InputsAreValid(string[] args)
        {
            return args != null
                && args.Length == 2
                && !string.IsNullOrWhiteSpace(args[0])
                && !string.IsNullOrWhiteSpace(args[1])
                && Directory.Exists(args[0]);
        }

        private static void Exit(string message = "", Exception e = null)
        {
            Console.WriteLine($"{message}.");
            
            if (e != null)
            {
                Console.WriteLine($"Exception : {e.Message}");
                Console.WriteLine($"Details : {e.StackTrace}");
            }

            Console.WriteLine("\n Press any key to exit the application.");
            Console.ReadLine();
            Environment.Exit(0);
        }

        private static string[] GetFilePathsToScan(string repertoryToScan)
        {
            string[] filepaths = [""];
            try
            {
                filepaths = Directory.GetFiles(repertoryToScan, "*.cs",
                     SearchOption.AllDirectories).Where(x => !(x.Contains("bin") || x.Contains("obj"))).ToArray();
            }
            catch (UnauthorizedAccessException uae)
            {
                Exit("The access to the repertory to scan is unauthorized", uae);
            }
            catch (PathTooLongException ptle)
            {
                Exit("The path to the repertory to scan is too long", ptle);
            }
            catch (Exception e)
            {
                Exit("Exception occured during the recuperation of the files to scan", e);
            }

            return filepaths;
        }

        private static string GetLogLevel(string line)
        {
            string pattern = @"\.(Information|Debug|Warning|Error)\(";

            Match match = Regex.Match(line, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private static string GetLogContent(string line)
        {
            string pattern = @"\(\""(.*?)\""\)";

            Match match = Regex.Match(line, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private static void DeleteFileIfExists(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        private static void ReadLogs(IEnumerable<Log> logs)
        {
            foreach (var log in logs)
            {
                ReadLog(log);
            }
        }

        private static void ReadLog(Log log)
        {
            Console.Write(log.LogLevel + " ");
            Console.WriteLine(log.LogContent + " ");
        }

        private static string ExportInTextFile(List<Log> logs, string filename)
        {
            using StreamWriter fileStream = new StreamWriter(filename, true);

            fileStream.WriteLine("### Log list\r\n");
            var currentLogLevel = logs.First().LogLevel;
            fileStream.WriteLine($"- **{currentLogLevel}**\r\n");
            
            ReadLogs(logs);
            
            foreach (var log in logs)
            {
                if (log.LogLevel != currentLogLevel)
                {
                    fileStream.WriteLine();
                    fileStream.WriteLine($"- **{log.LogLevel}**\r\n");
                    currentLogLevel = log.LogLevel;
                }
                fileStream.WriteLine($"\t- `{log.LogContent}`");
            }

            return ((FileStream)(fileStream.BaseStream)).Name;
        }
    }
}
