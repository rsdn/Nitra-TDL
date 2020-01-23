using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Tdl.Tests
{
    [TestFixture]
    public class TdlCompilerTests
    {
        private static readonly string BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string NetFrameworkToolPath = Path.Combine(BasePath, "net461", "tdlc.exe");
        private static readonly string NetCoreToolPath = Path.Combine(BasePath, "netcoreapp2.1", "tdlc.dll");
        private static readonly string ReferencesPath = Path.Combine(BasePath, "Tests");
        private static readonly string OutputPath = Path.Combine(Path.GetTempPath(), "tdlc-" + Guid.NewGuid());
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
        public void NetFramework(string directory) =>
            RunTest(directory, NetFrameworkToolPath);

        [TestCaseSource(nameof(GetTestCases))]
        public void NetCore(string directory) =>
            RunTest(directory, "dotnet", Utils.EscapeCommandLineArgument(NetCoreToolPath));

        private void RunTest(string directory, string executable, string firstArgument = null, [CallerMemberName] string scope = null)
        {
            directory = Path.Combine(Utils.TestsRootDirectory, directory);

            var name = scope + "-" + Path.GetFileNameWithoutExtension(directory);

            var arguments = new List<string>();
            if (!string.IsNullOrEmpty(firstArgument))
                arguments.Add(firstArgument);

            var sampleFilePath = Path.Combine(SamplesPath, name + ".json");
            if (File.Exists(sampleFilePath))
                arguments.Add($"-sample={Utils.EscapeCommandLineArgument(sampleFilePath)}");
            else
                arguments.Add("-compiler-messages-test");

            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
                arguments.Add(Utils.EscapeCommandLineArgument(Path.Combine(subDirectory, "*.tdl")));

            arguments.Add(Utils.EscapeCommandLineArgument(Path.Combine(ReferencesPath, "*.dll")));
            arguments.Add(typeof(System.Linq.Enumerable).Assembly.Location);

            var outputFilePath = Path.Combine(OutputPath, name + ".json");
            arguments.Add($"-out:{Utils.EscapeCommandLineArgument(outputFilePath)}");

            arguments.Add("-log-level=short");
            arguments.Add("-json-schema-type=prod");

            if (Debugger.IsAttached)
                arguments.Add("-debug");

            var result = Utils.RunProcess(directory, executable, string.Join(" ", arguments));

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
