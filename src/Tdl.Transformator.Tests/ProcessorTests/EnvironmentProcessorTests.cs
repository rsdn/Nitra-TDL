using System.IO;
using System.Linq;
using Tdl.Transformator.Models.Scenario;
using Tdl.Transformator.Processors;
using Tdl.Transformator.Tests.CommonServices;
using KL.TestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;

namespace Tdl.Transformator.Tests.ProcessorTests
{
    [TestClass]
    public class EnvironmentProcessorTests
    {
        private const string TestFolder = "ScenarioDuplicatesTdl";
        private static readonly string CurrentDirectory = Directory.GetCurrentDirectory();
        private static readonly string TdlDirectory = Path.Combine(TestUtils.TdlsRoot, TestFolder);
        
        [TestMethod]
        public void RemoveEnvironmentDuplicates()
        {
            var container = new ModelConverter().BuildContainer(TdlDirectory,
                Directory.GetFiles(TdlDirectory, "ScenarioDuplicatesEnvironments.tdl").Select(Path.GetFileName));
            var environments = container.GetAll<ScenarioModel>().SelectMany(scenario => scenario.Environments);
            var environmentProcessor = new EnvironmentProcessor();
            Assert.AreEqual(5, environments.Count());
            var mergedEnvironments = environmentProcessor.ConcatenateEnvironments(environments);
            Assert.AreEqual(2, mergedEnvironments.Count());
        }
    }
}
