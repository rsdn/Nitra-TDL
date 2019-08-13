using System.IO;
using System.Runtime.CompilerServices;

namespace Tdl.Transformator.Tests
{
    internal static class TestUtils
    {
        public static string TdlsRoot { get; }

        static TestUtils()
        {
            var path = CallerFilePath();
            TdlsRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), @"tdl"));
        }

        private static string CallerFilePath([CallerFilePath] string path = null) => path;
    }
}
