using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Tdl2JsonLib;

namespace Tdl2Json
{
    public static class Program
    {
        /// <summary>
        /// Generates JSON from source files <paramref name="sourceFileNames"/>. Throws <see cref="Exception"/> when error occures.
        /// </summary>
        public static string GenerateJson(string workingDirectory, IEnumerable<string> sourceFileNames, IEnumerable<string> references, bool validateMethodNames)
        {
            return JsonGenerator.Generate(workingDirectory, sourceFileNames.ToArray(), references.ToArray(), validateMethodNames);
        }

        /// <summary>
        /// Application entry point. Returns 0 for success.
        /// </summary>
        public static int Main(string[] args)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                if (args.Length == 1 && args[0].StartsWith("/from-file:", "-from-file:"))
                {
                    var arg = args[0];
                    int index = arg.IndexOf(':') + 1;
                    var comandOptionsFile = arg.Substring(index, arg.Length - index);
                    args = File.ReadAllLines(comandOptionsFile);
                }
                var outputs            = FilterOption(args, "/out:", "-out:");
                var workingDirectories = FilterOption(args, "/wd:", "-wd:", "/WorkingDirectory:", "-WorkingDirectory:");
                var files              = ExpandFilePaths(args.Where(a => !a.StartsWith("-", "/out:", "/wd:", "/h", "/?", "/from-file:"))).ToArray();
                var tdls               = files.Where(a => a.EndsWith(".tdl", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var refs               = files.Where(a => a.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var needHelp           = args.Contains("/?", "/h", "-?", "-h");

                if (args.Length == 0 || needHelp || outputs.Length > 1 || workingDirectories.Length > 1 || tdls.Length == 0)
                {
                    if (!needHelp)
                        Print("Invalid parameters", ConsoleColor.Red);
                    Print("Tdl2Json.exe [/out:output_file.json] file1.tdl[[+file2.tdl]...][[+file3.dll][+file4.dll]...]", ConsoleColor.Cyan);
                    Print(@"example Tdl2Json.exe /out:c:\temp\stage_tdl.json c:\TDLs\*.tdl c:\DLLs\Autotest.*.Suites.dll", ConsoleColor.Gray);

                    return needHelp ? 0 : -2;
                }
                var workingDirectory = workingDirectories.SingleOrDefault() ?? Directory.GetCurrentDirectory();
                var outPath = outputs.SingleOrDefault() ?? Path.Combine(workingDirectory, "out.json");
                if (tdls.Length > 10 || tdls.Sum(f => new FileInfo(f).Length) > 1024 * 1024)
                    JsonGenerator.OnMessage += JsonGenerator_OnMessage;
                var contents = JsonGenerator.Generate(workingDirectory, tdls, refs, isMethodTypingEnabled: true);
                File.WriteAllText(outPath, contents, Encoding.UTF8);
                Print($"The JSON file was created successfully: '{outPath}'.", ConsoleColor.Green);

                Console.WriteLine("Took: " + timer.Elapsed);
                return 0;
            }
            catch (CompilerErrorException e)
            {
                Print($"Compilation failed: {e.Message}", ConsoleColor.Red);
                Console.Error.Write(e.Message);
                return -3;
            }
            catch (ConfigurationException e)
            {
                Print($"Configuration error: {e.Message}", ConsoleColor.Red);
                Console.Error.Write($"Configuration error(1,1,1,1): error: {e.Message}", ConsoleColor.Red);
                return -3;
            }
            catch (Exception e)
            {
                Print($"Unhandled exception: {e}", ConsoleColor.Red);
                Console.Error.Write($"Unhandled exception(1,1,1,1): error: {e.Message}", ConsoleColor.Red);
                return -3;
            }
        }

        private static void JsonGenerator_OnMessage(string message)
        {
            Print(message, ConsoleColor.DarkGray);
        }

        private static string[] FilterOption(string[] args, params string[] samples)
        {
            return args.Where(a => a.StartsWith(samples)).Select(a => a.Substring(a.IndexOf(':') + 1)).ToArray();
        }

        private static void Print(string text, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }

        public static string[] ExpandFilePaths(IEnumerable<string> args)
        {
            var fileList = new List<string>();

            foreach (var arg in args)
            {
                var substitutedArg = System.Environment.ExpandEnvironmentVariables(arg);

                if (substitutedArg.Contains('*') || substitutedArg.Contains('?'))
                {
                    var dirPart = Path.GetDirectoryName(substitutedArg);
                    if (dirPart.Length == 0)
                        dirPart = ".";

                    var filePart = Path.GetFileName(substitutedArg);

                    foreach (var filepath in Directory.GetFiles(dirPart, filePart))
                        fileList.Add(filepath);

                    var dirs = Directory.GetDirectories(dirPart, "*.*", SearchOption.AllDirectories);
                    foreach (var dir in dirs)
                        foreach (var filepath in Directory.GetFiles(dir, filePart))
                            fileList.Add(filepath);
                }
                else
                    fileList.Add(substitutedArg);
            }

            return fileList.ToArray();
        }

        public static bool Contains(this string[] args, params string[] samples)
        {
            foreach (var arg    in args)
            foreach (var sample in samples)
            {
                if (arg.Contains(sample))
                    return true;
            }
            return false;
        }

        public static bool StartsWith(this string str, params string[] samples)
        {
            foreach (var sample in samples)
                if (str.StartsWith(sample, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        public static bool StartsWith(this string[] strs, params string[] samples)
        {
            foreach (var str    in strs)
            foreach (var sample in samples)
                if (str.StartsWith(sample, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
