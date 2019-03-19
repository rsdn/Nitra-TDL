using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestRunner
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    Print("Usages: TestRunner.exe path-to-test-root-dir path-to-test-samples-dit path-to-dlls-dit", ConsoleColor.Cyan);
                    return -1;
                }

                var pathToTetstRootDir   = args[0];
                var pathToTestSamplesDir = args[1];
                var pathToDllsDir        = args[2];

                if (!Directory.Exists(pathToTetstRootDir))
                {
                    PrintError($"The path-to-test-root-dir ('{pathToTetstRootDir}') does not exists.");
                    return -4;
                }

                if (!Directory.Exists(pathToTestSamplesDir))
                {
                    PrintError($"The path-to-test-samples-dit ('{pathToTestSamplesDir}') does not exists.");
                    return -4;
                }

                if (!Directory.Exists(pathToDllsDir))
                {
                    PrintError($"The path-to-dlls-dit ('{pathToDllsDir}') does not exists.");
                    return -4;
                }

                var timer          = Stopwatch.StartNew();
                var currentExePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                var testUtilPath   = Path.Combine(currentExePath, "Tdl2Json.exe");
                var outputDir      = Path.Combine(Path.GetTempPath(), "Tdl2Json-" + Guid.NewGuid());
                var dlls           = Path.Combine(pathToDllsDir, "*.dll");
                var failed             = 0;

                Directory.CreateDirectory(outputDir);

                foreach (var testDir in Directory.EnumerateDirectories(pathToTetstRootDir))
                {
                    var fileName   = Path.GetFileNameWithoutExtension(testDir);

                    if (fileName.Equals("bin", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    Console.Write(fileName + " ");

                    var outputFilePath        = Path.ChangeExtension(Path.Combine(outputDir, fileName), ".json");
                    var sampleFilePath        = Path.ChangeExtension(Path.Combine(pathToTestSamplesDir, fileName), ".json");
                    var isSampleFileExists    = File.Exists(sampleFilePath);
                    var option                = isSampleFileExists ? $"\"-sample={sampleFilePath}\"" : "-compiler-messages-test";
                    var tdls                  = Path.Combine(testDir, fileName, "*.tdl");
                    var arguments             = $"\"-out:{outputFilePath}\" \"{tdls}\" \"{dlls}\" --log-level=short {option}";
                    var startInfo             = new ProcessStartInfo();
                    startInfo.UseShellExecute = false;
                    startInfo.FileName        = testUtilPath;
                    startInfo.Arguments       = $"\"-out:{outputFilePath}\" \"{tdls}\" \"{dlls}\" --log-level=short {option}";
                    var process               = Process.Start(startInfo);
                    process.WaitForExit();
                    //var exitCode             = Tdl2Json.Program.Main(new[] { "\"-out:{outputFilePath}\"", "\"{tdls}\"", "\"{dlls}\"", "--log-level=short", option });
                    var exitCode = process.ExitCode;
                    if (exitCode < 0)
                    {
                        failed++;
                        //Console.WriteLine(arguments);
                    }
                }

                if (failed > 0)
                    PrintError($"{failed} tests failed! Test took: {timer.Elapsed}");
                else
                    Print("All tests passed.", ConsoleColor.Green);

                //Directory.Delete(outputDir, recursive: true);

                return failed > 0 ? -4 : 0;
            }
            finally
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
            }
        }

        private static void Print(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }

        private static void PrintError(string text)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }
    }
}
