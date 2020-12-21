﻿using Mono.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Tdl2Json
{
    public class CommandLineOptions
    {
        private static readonly Dictionary<string, MessageImportance> LogLevels =
            new Dictionary<string, MessageImportance>(StringComparer.OrdinalIgnoreCase)
            {
                ["normal"] = MessageImportance.Low,
                ["short"]  = MessageImportance.High,
            };

        private static readonly Dictionary<string, BooleanMarshalMode> BooleanMarshalModes =
            new Dictionary<string, BooleanMarshalMode>(StringComparer.OrdinalIgnoreCase)
            {
                ["boolean"] = BooleanMarshalMode.Boolean,
                ["integer"] = BooleanMarshalMode.Integer,
                ["string"] = BooleanMarshalMode.String,
            };

        private readonly OptionSet optionSet;

        public bool NeedHelp { get; private set; }

        public string OutputFile { get; private set; }

        public string WorkingDirectory { get; private set; }

        public MessageImportance LogLevel { get; private set; }

        public List<(string assembly, string type, string method)> Transformers { get; private set; }

        public List<string> InputFiles { get; private set; }

        public List<string> References { get; private set; }

        public List<string> IgnoredOptions { get; private set; }

        public bool ComilerMessagesTest { get; private set; }

        public string JsonSchemaType { get; private set; }

        public string SampleOutputFile { get; private set; }

        public string DeploymentScriptHeader { get; private set; }

        public BooleanMarshalMode BooleanMarshalMode { get; private set; }

        public string DiffFile { get; private set; }

        public bool ExcludeTriggerPathScenarios { get; private set; }

        public string RepositoryPath { get; private set; }

        public bool IsTestMode => ComilerMessagesTest || SampleOutputFile != null;

        public CommandLineOptions()
        {
            optionSet = new OptionSet()
            {
                { "?|h|help",                 "Prints this message.",                                    v => NeedHelp = true },
                { "o|out=",                   "Output file path.",                                       v => OutputFile = v },
                { "w|working-directory=",     "Working directory.",                                      v => WorkingDirectory = v },
                { "deployment-header=",       "Deployment script header.",                               v => DeploymentScriptHeader = v },
                { "l|log-level=",            $"Logging verbosity: {string.Join(", ", LogLevels.Keys)}.", SetLogLevel },
                { "t|transformator=",         "Transformer: path-to.dll|Qualified.Function.Name",        AddTransformer },
                { "m|compiler-messages-test", "Test compiler messages by samples.",                      _ => ComilerMessagesTest = true },
                { "s|sample=",                "Sample output file path.",                                v => SampleOutputFile = v },
                { "d|debug",                  "Start with debugger prompt",                              _ => Debugger.Launch() },
                { "b|bool-marshal-mode=",    $"Marshal mode for boolean values: {string.Join(", ", BooleanMarshalModes.Keys)}.", SetBooleanMarshalMode },
                { "json-schema-type=",        "JSON schema type.",                                       v => JsonSchemaType = v, true },
                { "diff-file=",               "Diff file path (--name-status format).",                  v => DiffFile = v },
                { "repo-directory=",          "Repository root path.",                                   v => RepositoryPath = v },
                { "exclude-triggerpath-scenarios", "Include only scenarios without TriggerPath attribute.", v => ExcludeTriggerPathScenarios = true },
                new ResponseFileSource(),

                // backward compatibility
                { "wd|workingDirectory=", "", v => WorkingDirectory = v, true },
            };
        }

        public void Parse(IEnumerable<string> arguments)
        {
            LogLevel = MessageImportance.Low;
            BooleanMarshalMode = BooleanMarshalMode.Integer;
            Transformers = new List<(string, string, string)>();
            InputFiles = new List<string>();
            References = new List<string>();
            IgnoredOptions = new List<string>();

            var extraValues = optionSet.Parse(arguments);

            if (NeedHelp)
                return;

            foreach (var value in ExpandFilePaths(extraValues))
            {
                const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
                if (value.EndsWith(".tdl", comparison))
                    InputFiles.Add(value);
                else if (value.EndsWith(".dll", comparison) || value.EndsWith(".exe", comparison))
                    References.Add(value);
                else
                    IgnoredOptions.Add(value);
            }

            if (string.IsNullOrEmpty(WorkingDirectory))
                WorkingDirectory = Directory.GetCurrentDirectory();

            if (string.IsNullOrEmpty(OutputFile))
                OutputFile = Path.Combine(WorkingDirectory, "out.json");

            if (InputFiles.Count == 0)
                throw new OptionException("At least one input TDL file required.", "");

            if (string.IsNullOrEmpty(DiffFile) != string.IsNullOrEmpty(RepositoryPath))
                throw new OptionException("Both diff file and repository root paths must be specified.", "");
        }

        public void PrintHelp(TextWriter writer)
        {
            writer.WriteLine("Usage: Tdl2Json [OPTIONS] file1.tdl[[+file2.tdl]...][[+file3.dll][+file4.dll]...]");
            optionSet.WriteOptionDescriptions(writer);
            writer.WriteLine(@"Example: Tdl2Json /out:c:\temp\stage_tdl.json c:\TDLs\*.tdl c:\DLLs\Autotest.*.Suites.dll");
        }

        private void SetLogLevel(string value)
        {
            if (LogLevels.TryGetValue(value, out var logLevel))
                LogLevel = logLevel;
            else
                throw new OptionException($"Invalid logging verbosity level '{value}'", "log-level");
        }

        private void SetBooleanMarshalMode(string value)
        {
            if (BooleanMarshalModes.TryGetValue(value, out var mode))
                BooleanMarshalMode = mode;
            else
                throw new OptionException($"Invalid invalid boolean marshal mode '{value}'", "bool-marshal-mode");
        }

        private void AddTransformer(string value)
        {
            var typeSeparatorIndex = value.LastIndexOf('|');
            var methodSeparatorIndex = value.LastIndexOf('.');
            if (typeSeparatorIndex <= 0 || methodSeparatorIndex < 0 || typeSeparatorIndex + 1 >= methodSeparatorIndex
                || methodSeparatorIndex == value.Length - 1)
            {
                throw new OptionException($"Invalid transformer value '{value}'", "transformer");
            }

            var assembly  = value.Substring(0, typeSeparatorIndex);
            var typeStart = typeSeparatorIndex + 1;
            var type      = value.Substring(typeStart, methodSeparatorIndex - typeStart);
            var method    = value.Substring(methodSeparatorIndex + 1);

            Transformers.Add((assembly, type, method));
        }

        private static IEnumerable<string> ExpandFilePaths(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                var substitutedArg = Environment.ExpandEnvironmentVariables(arg);

                if (substitutedArg.Contains('*') || substitutedArg.Contains('?'))
                {
                    var dirPart = Path.GetDirectoryName(substitutedArg);
                    if (dirPart.Length == 0)
                        dirPart = ".";

                    var filePart = Path.GetFileName(substitutedArg);

                    foreach (var filepath in Directory.GetFiles(dirPart, filePart))
                        yield return filepath;

                    var dirs = Directory.GetDirectories(dirPart, "*.*", SearchOption.AllDirectories);
                    foreach (var dir in dirs)
                        foreach (var filepath in Directory.GetFiles(dir, filePart))
                            yield return filepath;
                }
                else
                    yield return substitutedArg;
            }
        }
    }
}
