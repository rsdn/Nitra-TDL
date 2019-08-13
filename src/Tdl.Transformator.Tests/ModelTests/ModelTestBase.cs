using Tdl.Transformator.Services;
using KL.TestFramework;
using System.IO;
using Tdl.Transformator.Tests.CommonServices;
using Unity;
using System.Runtime.CompilerServices;

namespace Tdl.Transformator.Tests.ModelTests
{
    public abstract class ModelTestBase
    {
        private static readonly string CurrentDirectory = Directory.GetCurrentDirectory();

        public readonly ModelConverter _modelConverter = new ModelConverter();
        
        public string GetExpectedTdl(string tdlFiles)
        {
            var path = Path.Combine(DirectoryExpectedFiles, tdlFiles);
            return File.ReadAllText(path);
        }

        protected abstract string TdlFolder { get; }

        protected string DirectoryFiles => Path.Combine(TestUtils.TdlsRoot, TdlFolder);

        protected string DirectoryExpectedFiles => Path.Combine(DirectoryFiles, "ExpectedTdl");

        protected string DirectoryInputFiles => Path.Combine(DirectoryFiles, "InputTdl");

        protected ModelContainer BuildContainer(params string[] tdlFiles)
        {
            return _modelConverter.BuildContainer(DirectoryInputFiles, tdlFiles);
        }
    }
}
