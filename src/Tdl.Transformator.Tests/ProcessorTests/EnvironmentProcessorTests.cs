using System.IO;
using System.Linq;
using KL.TdlTransformator.Models.Scenario;
using KL.TdlTransformator.Processors;
using KL.TdlTransformator.Tests.CommonServices;
using KL.TestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;

namespace KL.TdlTransformator.Tests.ProcessorTests
{
    [TestClass]
    public class EnvironmentProcessorTests
    {
        private const string TdlFolder = "tdl";
        private const string TestFolder = "ScenarioDuplicatesTdl";
        private static readonly string CurrentDirectory = Directory.GetCurrentDirectory();
        private static readonly string TdlDirectory = Path.Combine(CurrentDirectory, TdlFolder, TestFolder);
        
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
