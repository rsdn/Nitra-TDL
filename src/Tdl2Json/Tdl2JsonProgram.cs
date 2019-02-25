using Mono.Options;
using Nitra;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Tdl2Json
{
    public static class Program
    {
        internal const int MaxErrorsOrWarningsToDisplay = 30;

        /// <summary>
        /// Application entry point. Returns 0 for success.
        /// </summary>
        public static int Main(string[] args)
        {
            var timer = Stopwatch.StartNew();
            var options = new CommandLineOptions();
            try
            {
                var toolName = Assembly.GetExecutingAssembly().GetName();
                Print($"{toolName.Name} v{toolName.Version}", ConsoleColor.Gray);

                options.Parse(args);

                if (options.NeedHelp)
                {
                    options.PrintHelp(Console.Out);
                    return 0;
                }

                JsonGenerator.OnMessage += (message, importance) =>
                {
                    if (importance >= options.LogLevel)
                        Print(message, ConsoleColor.DarkGray);
                };

                foreach (var ignoredOption in options.IgnoredOptions)
                    PrintError($"File or option was ignored: {ignoredOption}");

                var (tdls, refs) = (options.InputFiles.ToArray(), options.References.ToArray());
                var output = new Lazy<TextWriter>(() => new StreamWriter(options.OutputFile, false, Encoding.UTF8));
                CompilerMessageBag messages;

                if (options.Transformers.Count > 0)
                {
                    Print($"Using custom transformers", ConsoleColor.Magenta);
                    messages = new CompilerMessageBag();
                    foreach (var t in options.Transformers)
                    {
                        var transformatorFunc = LoadTransformator(t.assembly, t.type, t.method);
                        messages.AddRange(JsonGenerator.Generate(options.WorkingDirectory, tdls, refs, isMethodTypingEnabled: true, output: null,
                            transformatorOutput: options.OutputFile, transformatorOpt: transformatorFunc));

                        if (messages.HasErrors)
                            break;
                    }
                }
                else
                {
                    messages = JsonGenerator.Generate(options.WorkingDirectory, tdls, refs, isMethodTypingEnabled: true, output: output,
                        transformatorOutput: null, transformatorOpt: null);
                }

                if (output.IsValueCreated)
                    output.Value.Dispose();

                ReportCompilerMessages(messages);

                if (output.IsValueCreated)
                    Print($"JSON file was created successfully: '{options.OutputFile}'.", ConsoleColor.Green);

                Print("Took: " + timer.Elapsed, ConsoleColor.DarkGray);
                return messages.HasErrors ? -3 : 0;
            }
            catch (OptionException e)
            {
                PrintError(e.Message);
                return -2;
            }
            catch (ConfigurationException e)
            {
                PrintError($"Configuration error: {e.Message}");
                return -3;
            }
            catch (Exception e)
            {
                PrintError($"Unhandled exception: {e}");
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

        private static void ReportCompilerMessages(CompilerMessageBag messages)
        {
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

        private static TransfomationFunc LoadTransformator(string path, string typeName, string methodName)
        {
            var assembly = Assembly.LoadFrom(path);
            var type = assembly.GetType(typeName, throwOnError: true);
            return (TransfomationFunc)Delegate.CreateDelegate(typeof(TransfomationFunc), type, methodName);
        }

        private static void Print(string text, ConsoleColor color)
        {
            using (new ConsoleForegroundColor(color))
                Console.WriteLine(text);
        }
    }
}
