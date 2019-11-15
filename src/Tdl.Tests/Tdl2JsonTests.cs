using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tdl.Tests
{
    [TestFixture]
    public class Tdl2JsonTests
    {
        private static readonly string BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string ToolPath = Path.Combine(BasePath, "Tdl2Json.exe");
        private static readonly string ReferencesPath = Path.Combine(BasePath, "Tests");
        private static readonly string OutputPath = Path.Combine(Path.GetTempPath(), "Tdl2Json-" + Guid.NewGuid());
        private static readonly string SamplesPath = Path.Combine(Utils.TestsRootDirectory, "Samples");

        [SetUp]
        public void Setup() =>
            Directory.CreateDirectory(OutputPath);

        [TearDown]
        public void TearDown()
        {
            //Directory.Delete(OutputPath, recursive: true);
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void Tdl(string directory)
        {
            directory = Path.Combine(Utils.TestsRootDirectory, directory);

            var name = Path.GetFileNameWithoutExtension(directory);

            var arguments = new List<string>();

            var sampleFilePath = Path.Combine(SamplesPath, name + ".json");
            if (File.Exists(sampleFilePath))
                arguments.Add($"-sample={Utils.EscapeCommandLineArgument(sampleFilePath)}");
            else
                arguments.Add("-compiler-messages-test");

            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
                arguments.Add(Utils.EscapeCommandLineArgument(Path.Combine(subDirectory, "*.tdl")));

            arguments.Add(Utils.EscapeCommandLineArgument(Path.Combine(ReferencesPath, "*.dll")));

            var outputFilePath = Path.Combine(OutputPath, name + ".json");
            arguments.Add($"-out:{Utils.EscapeCommandLineArgument(outputFilePath)}");

            arguments.Add("-log-level=short");
            arguments.Add("-json-schema-type=prod");

            if (Debugger.IsAttached)
                arguments.Add("-debug");

            var result = Utils.RunProcess(directory, ToolPath, string.Join(" ", arguments));

            if (File.Exists(outputFilePath))
            {
                TestContext.Out.WriteLine("Output:");
                TestContext.Out.WriteLine(File.ReadAllText(outputFilePath));
            }

            Assert.AreEqual(0, result.ExitCode, "Exit code");
            Assert.IsFalse(result.HasStdError, "Stderr");
        }

        private static IEnumerable<string> GetTestCases() =>
            Directory.EnumerateDirectories(Path.Combine(Utils.TestsRootDirectory, "Tdl"))
                .Select(s => s.Substring(Utils.TestsRootDirectory.Length + 1));
    }
}
