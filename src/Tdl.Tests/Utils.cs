using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Tdl.Tests
{
    internal static class Utils
    {
        public static readonly string TestsRootDirectory = Path.GetFullPath(Path.Combine(CallerFilePath(), @"..\..\..\Tests"));

        public static (int ExitCode, bool HasStdError) RunProcess(string workingDirectory, string fileName, string arguments)
        {
            TestContext.WriteLine($"{fileName} {arguments}");

            var hasStdError = false;
            var locker = new SpinLock();

            DataReceivedEventHandler CreateDataHandler(bool isError) => (sender, e) =>
            {
                if (e.Data is null)
                    return;

                var lockTaken = false;
                try
                {
                    locker.Enter(ref lockTaken);

                    if (isError)
                        hasStdError = true;

                    TestContext.Out.WriteLine(e.Data);
                }
                finally
                {
                    if (lockTaken)
                        locker.Exit();
                }
            };

            using (var process = new Process())
            {
                var startInfo = process.StartInfo;
                startInfo.FileName = fileName;
                startInfo.Arguments = arguments;
                startInfo.WorkingDirectory = workingDirectory;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;

                process.ErrorDataReceived += CreateDataHandler(true);
                process.OutputDataReceived += CreateDataHandler(false);

                try
                {
                    process.Start();

                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    process.WaitForExit();
                }
                catch (ThreadAbortException)
                {
                    try { if (!process.HasExited) process.Kill(); } catch { }
                }

                return (process.ExitCode, hasStdError);
            }
        }

        public static string CallerFilePath([CallerFilePath] string path = null) => path;

        public static string EscapeCommandLineArgument(string value) => 
            value.IndexOf(' ') >= 0 ? ("\"" + value + "\"") : value;
    }
}
