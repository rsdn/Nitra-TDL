using Nitra;
using Nitra.ProjectSystem;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Tdl2JsonLib;

using File = System.IO.File;

namespace Tdl2Json
{
    public static class Program
    {
        internal const int MaxErrorsOrWarningsToDisplay = 30;
        private const string TransformPrompt = "path-to.dll|Qualified.Function.Name";

        private static readonly Dictionary<string, MessageImportance> LogLevels = new Dictionary<string, MessageImportance>(StringComparer.OrdinalIgnoreCase)
        {
            ["normal"] = MessageImportance.Low,
            ["short"]  = MessageImportance.High,
        };

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
                    var arg               = args[0];
                    int index             = arg.IndexOf(':') + 1;
                    var comandOptionsFile = arg.Substring(index, arg.Length - index);
                    args                  = File.ReadAllLines(comandOptionsFile);
                }
                var transformators     = FilterOption(args, "/transformator:", "-transformator:");
                var outputs            = FilterOption(args, "/out:", "-out:");
                var workingDirectories = FilterOption(args, "/wd:", "-wd:", "/WorkingDirectory:", "-WorkingDirectory:");
                var loglevels          = FilterOption(args, "/log-level:", "-log-level:");
                var files              = ExpandFilePaths(args.Where(a => !a.StartsWith("-", "/out:", "/wd:", "/h", "/?", "/from-file:", "/transformator:", "/log-level:"))).ToArray();
                var tdls               = files.Where(a => a.EndsWith(".tdl", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var refs               = files.Where(a => a.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var needHelp           = args.Contains("/?", "/h", "-?", "-h");

                if (args.Length == 0 || needHelp || outputs.Length > 1 || workingDirectories.Length > 1 || tdls.Length == 0 || loglevels.Length > 1)
                {
                    if (!needHelp)
                        Print("Invalid parameters", ConsoleColor.Red);
                    Print($"Tdl2Json.exe [-out:output_file.json] file1.tdl[[+file2.tdl]...][[+file3.dll][+file4.dll]...] [-transformator:{TransformPrompt}] [-log-level:normal]", ConsoleColor.Cyan);
                    Print(@"example Tdl2Json.exe /out:c:\temp\stage_tdl.json c:\TDLs\*.tdl c:\DLLs\Autotest.*.Suites.dll", ConsoleColor.Gray);

                    return needHelp ? 0 : -2;
                }

                var logLevel = MessageImportance.Low;
                if (loglevels.Length == 1 && !LogLevels.TryGetValue(loglevels[0], out logLevel))
                {
                    Print($"Invalid log level value '{loglevels[0]}'. Valid values are: {string.Join(", ", LogLevels.Keys)}.", ConsoleColor.Cyan);
                    return -2;
                }

                var workingDirectory = workingDirectories.SingleOrDefault() ?? Directory.GetCurrentDirectory();
                var outPath = outputs.SingleOrDefault() ?? Path.Combine(workingDirectory, "out.json");

                JsonGenerator.OnMessage += (message, importance) =>
                {
                    if (importance >= logLevel)
                        Print(message, ConsoleColor.DarkGray);
                };

                CompilerMessageBag messages;

                if (transformators.Length > 0)
                {
                    Print($"Used a custom transformers", ConsoleColor.Magenta);
                    messages = new CompilerMessageBag();
                    foreach (string transformator in transformators)
                    {
                        var transformatorFunc = LoadTransformator(transformator);
                        messages.AddRange(JsonGenerator.Generate(workingDirectory, tdls, refs, isMethodTypingEnabled: true, output: null,
                            transformatorOutput: outPath, transformatorOpt: transformatorFunc));

                        if (messages.HasErrors)
                            break;
                    }
                    ReportCompilerMessages(messages);
                }
                else
                {
                    var output = new Lazy<TextWriter>(() => new StreamWriter(outPath, false, Encoding.UTF8));
                    messages = JsonGenerator.Generate(workingDirectory, tdls, refs, isMethodTypingEnabled: true, output: output,
                        transformatorOutput: null, transformatorOpt: null);
                    if (output.IsValueCreated)
                        output.Value.Dispose();
                    ReportCompilerMessages(messages);
                    if (!messages.HasErrors)
                        Print($"The JSON file was created successfully: '{outPath}'.", ConsoleColor.Green);
                }

                if (messages.HasErrors)
                    return -3;

                Console.WriteLine("Took: " + timer.Elapsed);
                return 0;
            }
            catch (ConfigurationException e)
            {
                using (new ConsoleForegroundColor(ConsoleColor.Red))
                    Console.Error.Write($"Configuration error: {e.Message}");
                return -3;
            }
            catch (Exception e)
            {
                using (new ConsoleForegroundColor(ConsoleColor.Red))
                    Console.Error.Write($"Unhandled exception: {e}");
                return -3;
            }
        }

        private static void PrintError(string msg)
        {
            using (new ConsoleForegroundColor(ConsoleColor.Red))
                Console.Error.WriteLine(msg);
        }

        private static void PrintWarning(string msg)
        {
            using (new ConsoleForegroundColor(ConsoleColor.Yellow))
                Console.Error.WriteLine(msg);
        }

        private static void PrintHint(string msg)
        {
            using (new ConsoleForegroundColor(ConsoleColor.DarkYellow))
                Console.Error.WriteLine(msg);
        }

        private static void ReportCompilerMessages(IEnumerable<CompilerMessage> messages)
        {
            var texts        = new List<string>();
            var errorCount   = 0;
            var warningCount = 0;
            var skipHints    = false;

            foreach (var message in messages)
            {
                if (message.Type == CompilerMessageType.FatalError || message.Type == CompilerMessageType.Error)
                {
                    if (errorCount == MaxErrorsOrWarningsToDisplay)
                    {
                        PrintWarning($"Too many errors detected! Only first {errorCount} shown.");
                        break;
                    }
                    errorCount++;
                    skipHints = false;
                    PrintError(message.ToString());
                }
                else if (message.Type == CompilerMessageType.Warning)
                {
                    if (warningCount == MaxErrorsOrWarningsToDisplay)
                        PrintWarning($"Too many warnings detected! Only first {warningCount} shown.");
                    warningCount++;
                    if (warningCount > MaxErrorsOrWarningsToDisplay)
                    {
                        skipHints = true;
                        continue;
                    }
                    PrintWarning(message.ToString());
                }
                else if (message.Type == CompilerMessageType.Hint && !skipHints)
                    PrintHint(message.ToString());
            }
        }

        static (string path, string func) getPathAndFunc(string transformator)
        {
            var index = transformator.LastIndexOf("|");
            if (index < 0)
                ReportIncorrectTransformator();
            var path = transformator.Substring(0, index);
            var func = transformator.Substring(index + 1, transformator.Length - (index + 1));
            return (path, func);
        }

        static (Type type, string funcName) getTypeAndFunc(Assembly asm, string qualifiedFunctionName)
        {
            var index = qualifiedFunctionName.LastIndexOf(".");
            if (index < 0)
                ReportIncorrectTransformator();
            var typeName = qualifiedFunctionName.Substring(0, index);
            var type     = asm.GetType(typeName, throwOnError: true);
            var funcName = qualifiedFunctionName.Substring(index + 1, qualifiedFunctionName.Length - (index + 1));
            return (type, funcName);
        }

        private static TransfomationFunc LoadTransformator(string transformator)
        {
            var (path, qualifiedFunctionName) = getPathAndFunc(transformator);
            var (type, funcName) = getTypeAndFunc(Assembly.LoadFrom(path), qualifiedFunctionName);
            return (TransfomationFunc)Delegate.CreateDelegate(typeof(TransfomationFunc), type, funcName);
        }

        private static void ReportIncorrectTransformator()
        {
            throw new Exception($"The 'transformator' parameter should have the format: '{TransformPrompt}'.");
        }

        private static string[] FilterOption(string[] args, params string[] samples)
        {
            return args.Where(a => a.StartsWith(samples)).Select(a => a.Substring(a.IndexOf(':') + 1)).ToArray();
        }

        private static void Print(string text, ConsoleColor color)
        {
            using (new ConsoleForegroundColor(color))
                Console.WriteLine(text);
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
