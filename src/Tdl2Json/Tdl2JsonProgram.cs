using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Tdl2JsonLib;

namespace Tdl2Json
{
    using TransformatorType = Func<DotNet.NamespaceSymbol, string, List<Nitra.ProjectSystem.CompilerMessage>>;

    public static class Program
    {
        private const string TransformPrompt = "path-to.dll|Qualified.Function.Name";

        /// <summary>
        /// Generates JSON from source files <paramref name="sourceFileNames"/>. Throws <see cref="Exception"/> when error occures.
        /// </summary>
        public static string GenerateJson(string workingDirectory, IEnumerable<string> sourceFileNames, IEnumerable<string> references, bool validateMethodNames)
        {
            return JsonGenerator.Generate(workingDirectory, sourceFileNames.ToArray(), references.ToArray(), "", validateMethodNames, null);
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
                var transformators     = FilterOption(args, "/transformator:", "-transformator:");
                var outputs            = FilterOption(args, "/out:", "-out:");
                var workingDirectories = FilterOption(args, "/wd:", "-wd:", "/WorkingDirectory:", "-WorkingDirectory:");
                var files              = ExpandFilePaths(args.Where(a => !a.StartsWith("-", "/out:", "/wd:", "/h", "/?", "/from-file:", "/transformator:"))).ToArray();
                var tdls               = files.Where(a => a.EndsWith(".tdl", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var refs               = files.Where(a => a.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var needHelp           = args.Contains("/?", "/h", "-?", "-h");

                if (args.Length == 0 || needHelp || outputs.Length > 1 || workingDirectories.Length > 1 || tdls.Length == 0)
                {
                    if (!needHelp)
                        Print("Invalid parameters", ConsoleColor.Red);
                    Print($"Tdl2Json.exe [-out:output_file.json] file1.tdl[[+file2.tdl]...][[+file3.dll][+file4.dll]...] [-transformator:{TransformPrompt}]", ConsoleColor.Cyan);
                    Print(@"example Tdl2Json.exe /out:c:\temp\stage_tdl.json c:\TDLs\*.tdl c:\DLLs\Autotest.*.Suites.dll", ConsoleColor.Gray);

                    return needHelp ? 0 : -2;
                }
                var workingDirectory = workingDirectories.SingleOrDefault() ?? Directory.GetCurrentDirectory();
                var outPath = outputs.SingleOrDefault() ?? Path.Combine(workingDirectory, "out.json");
                if (tdls.Length > 10 || tdls.Sum(f => new FileInfo(f).Length) > 1024 * 1024)
                    JsonGenerator.OnMessage += JsonGenerator_OnMessage;
                if (transformators.Length > 0)
                {
                    Print($"Used a custom transformers", ConsoleColor.Magenta);
                    foreach (string transformator in transformators)
                    {
                        var transformatorFunc = LoadTransformator(transformator);
                        JsonGenerator.Generate(workingDirectory, tdls, refs, outPath, isMethodTypingEnabled: true, transformatorOpt: transformatorFunc);
                    }
                }
                else
                {
                    var contents = JsonGenerator.Generate(workingDirectory, tdls, refs, outPath, isMethodTypingEnabled: true, transformatorOpt: null);
                    File.WriteAllText(outPath, contents, Encoding.UTF8);
                    Print($"The JSON file was created successfully: '{outPath}'.", ConsoleColor.Green);
                }

                Console.WriteLine("Took: " + timer.Elapsed);
                return 0;
            }
            catch (CompilerErrorException e)
            {
                using (new ConsoleForegroundColor(ConsoleColor.Red))
                {
                    Console.WriteLine("Compilation failed:");
                    Console.Error.Write(e.Message);
                }
                return -3;
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

        private static TransformatorType LoadTransformator(string transformator)
        {
            var (path, qualifiedFunctionName) = getPathAndFunc(transformator);
            var (type, funcName) = getTypeAndFunc(Assembly.LoadFrom(path), qualifiedFunctionName);
            return (TransformatorType)Delegate.CreateDelegate(typeof(TransformatorType), type, funcName);
        }

        private static void ReportIncorrectTransformator()
        {
            throw new Exception($"The 'transformator' parameter should have the format: '{TransformPrompt}'.");
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
