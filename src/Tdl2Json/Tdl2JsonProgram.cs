using Mono.Options;
using Nitra;
using Nitra.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using File = System.IO.File;

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

                options.Parse(args);
                var isTestMode = options.IsTestMode;

                if (!isTestMode)
                    Print($"{toolName.Name} v{toolName.Version}", ConsoleColor.Gray);


                if (options.NeedHelp)
                {
                    options.PrintHelp(Console.Out);
                    return 0;
                }


                JsonGenerator.OnMessage += (message, importance) =>
                {
                    if (!isTestMode && importance >= options.LogLevel)
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
                        var transformationContext = JsonGenerator.Generate(options.WorkingDirectory, tdls, refs, isMethodTypingEnabled: true, output: null,
                            transformatorOutput: options.OutputFile, transformatorOpt: transformatorFunc, isTestMode: true);
                        messages.AddRange(transformationContext.Messages);

                        if (messages.HasErrors)
                            break;
                    }
                }
                else
                {
                    var transformationContext = JsonGenerator.Generate(options.WorkingDirectory, tdls, refs, isMethodTypingEnabled: true, output: output,
                        transformatorOutput: null, transformatorOpt: null, isTestMode: isTestMode);
                    Debug.Assert(transformationContext.Comments != null);

                    if (isTestMode)
                    {
                        if (!TestCompilerMessages(transformationContext, options))
                            return -4;
                        if (output.IsValueCreated && options.SampleOutputFile != null)
                        {
                            var result = TestCompareWitSample(options, output);
                            if (result < 0)
                                return result;
                        }

                        Print($"test passed. Test took: {timer.Elapsed}", ConsoleColor.Green);
                        return 0;
                    }

                    messages = transformationContext.Messages;
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
            finally
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
        }

        private static int TestCompareWitSample(CommandLineOptions options, Lazy<TextWriter> output)
        {
            output.Value.Dispose();

            if (!File.Exists(options.SampleOutputFile))
            {
                PrintError($"Test failed: Sample file '{options.SampleOutputFile}' not exists.");
                return -4;
            }

            if (!File.Exists(options.OutputFile))
            {
                PrintError($"Test failed: Output file '{options.OutputFile}' not exists.");
                return -4;
            }

            var resultOpt = Tests.Diff(options.OutputFile, options.SampleOutputFile);
            if (resultOpt != null)
            {
                Print("Test failed: The output file not equals with the sample output file.", ConsoleColor.Red);
                PrintTestOutputAndSample(options);
                PrintDiff(resultOpt.ToString());
                return -4;
            }
            return 0;
        }

        private static bool TestCompilerMessages(TransformationContext transformationContext, CommandLineOptions options)
        {
            var result = Tests.CheckCompilerMessages(transformationContext);

            if (result.HasUnmathed)
            {
                if (!result.NotMathedMessages.IsEmpty)
                {
                    PrintError("Test failed: Unexpected compiler messages present.");
                    PrintTestOutputAndSample(options);
                    ReportCompilerMessages(result.NotMathedMessages);
                }

                if (!result.NotMathedSamples.IsEmpty)
                {
                    PrintError($"Test failed: {result.NotMathedSamples.Length} sample compiler messages don't match the compiler messages.");
                    PrintTestOutputAndSample(options);
                    foreach (var sample in result.NotMathedSamples)
                        PrintError($"{sample.Location.ToMessageString()}    The {sample.Kind} not match with appropriate compiler message: '{sample.Body}'.");
                }

                return false;
            }

            return true;
        }

        private static void PrintTestOutputAndSample(CommandLineOptions options)
        {
            PrintHint($"      OutputFile: '{options.OutputFile}'.");
            PrintHint($"SampleOutputFile: '{options.SampleOutputFile}'.");
        }

        private static void PrintDiff(string msg)
        {
            using (new ConsoleForegroundColor(ConsoleColor.Magenta))
                Console.Error.WriteLine(msg);
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
            var errorCount   = 0;
            var warningCount = 0;
            var skipHints    = false;

            foreach (Nitra.ProjectSystem.CompilerMessage message in messages)
                if (PrintCompilerMessage(message, ref errorCount, ref warningCount, ref skipHints))
                    break;
        }

        private static bool PrintCompilerMessage(Nitra.ProjectSystem.CompilerMessage message, ref int errorCount, ref int warningCount, ref bool skipHints)
        {
            if (message.Type == CompilerMessageType.FatalError || message.Type == CompilerMessageType.Error)
            {
                if (errorCount == MaxErrorsOrWarningsToDisplay)
                {
                    PrintWarning($"Too many errors detected! Only first {errorCount} shown.");
                    return true;
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
                    return false;
                }
                PrintWarning(message.ToString());
            }
            else if (message.Type == CompilerMessageType.Hint && !skipHints)
                PrintHint(message.ToString());

            foreach (var nestedMessage in message.NestedMessages)
                if (PrintCompilerMessage(nestedMessage, ref errorCount, ref warningCount, ref skipHints))
                    return true;

            return false;
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
