using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Modules;
using Tdl.Transformator.Models.Scenario;
using Tdl.Transformator.Models.Suite;
using Tdl.Transformator.Processors;
using Tdl.Transformator.Services;
using Tdl.Transformator.Tests.CommonServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tdl.Transformator.Tests.ProcessorTests
{
    [TestClass]
    public class ScenarioProcessorTests
    {
        public readonly ModelConverter _modelConverter = new ModelConverter();

        [TestMethod]
        public void RemoveTotalDuplicates()
        {
            const string testFolder = "ScenarioDuplicatesTdl";

            var dir = TdlDirectory(testFolder);
            var container = _modelConverter.BuildContainer(dir, Directory.GetFiles(dir, "ScenarioTotalDuplicates.Tdl").Select(Path.GetFileName));
            var processor = new ScenarioProcessor();
            var suiteBefore = container.GetAll<SuiteDefModel>().First(model => model.Name == "KIS_dupe_only");

            Assert.AreEqual("DuplicateTest-1", suiteBefore.Statements.First().Name);
            processor.ReplaceDuplicates(container.GetAll<ScenarioModel>());

            var scenarios = container.GetAll<ScenarioBaseModel>();
            Assert.AreEqual(6, scenarios.Count(), "only two scenarios should be present");

            ValidateScenarioSet(
                scenarios,
                "ScenarioWithTwoDupes",
                new[] { "OriginalTest-1" });
            ValidateScenarioSet(
                scenarios,
                "ScenarioNoDupes",
                new[] { "OriginalTest-1", "OriginalTest-2" });
            ValidateScenarioSet(
                scenarios,
                "ScenarioWithDuplicate",
                new[] { "OriginalTest-2", "OriginalTest-1" });

            var module = container.GetAll<ModuleModel>().First();
            Assert.AreEqual(0, module.Items.Count(item => item.Name == "DuplicateTest-1"));

            var suite = container.GetAll<SuiteDefModel>().First(model => model.Name == "KIS_dupe_only");
            Assert.AreEqual("OriginalTest-1", suite.Statements.First().Name);
        }

        [TestMethod]
        public void UngroupScenarioSequence()
        {
            const string testFolder = "UngroupingScenarioSequence";
            const string tdl = "Ungrouping.tdl";
            const string excpectedTdl = "Ungrouping_After.tdl";

            void action(ModelContainer container) => new ScenarioProcessor().Ungrouping(container, null);

            CompareModuleAfterAction(testFolder, tdl, excpectedTdl, action); 
        }

        [TestMethod]
        public void UngroupScenarioSequenceByQcId()
        { 
            const string testFolder = "UngroupingScenarioSequence";
            const string tdl = "UngroupingByQcId.tdl";
            const string excpectedTdl = "UngroupingByQcId_After.tdl";

            void action(ModelContainer container) => new ScenarioProcessor().UngroupingByQcId(container);

            CompareModuleAfterAction(testFolder, tdl, excpectedTdl, action); 
        }

        private static string TdlDirectory([NotNull] string testFolder) => Path.Combine(TestUtils.TdlsRoot, testFolder);  

        private static void ValidateScenarioSet(
            IEnumerable<ScenarioBaseModel> scenarios,
            string setName,
            string[] referenceNames)
        {
            var checkedScenarioSet = scenarios.Where(scenario => scenario.Name == setName);
            Assert.AreEqual(1, checkedScenarioSet.Count(),
                $"only one scenario {setName} should be present");
            var set = checkedScenarioSet.First() as ScenarioSetModel;
            Assert.AreEqual(referenceNames.Length, set.Members.Count, "incorrect number of referenced scenarios");

            for (var i = 0; i < referenceNames.Length; i++)
            {
                Assert.AreEqual(referenceNames[i], set.Members[i].Model.Name,
                    "incorrect scenario");
            }
        }

        private void CompareModuleAfterAction(
            [NotNull] string tdlDir, 
            [NotNull] string originalTdlFile, 
            [NotNull] string excpectedTdlFile,
            [NotNull] Action<ModelContainer> actionBeforeCompare)
        {
            var dir = TdlDirectory(tdlDir);
            var tdl = Path.Combine(dir, originalTdlFile);
            var excpectedTdl = Path.Combine(dir, excpectedTdlFile);

            var container = _modelConverter.BuildContainer(dir, new[] { tdl });
            var containerAfter = _modelConverter.BuildContainer(dir, new[] { excpectedTdl });

            actionBeforeCompare(container);

            DeleteAllSceanriosPostfix(container);

            var module = container.GetPrintable().Single();
            var moduleAfter = containerAfter.GetPrintable().Single();
            var areModulesIdentical = module.IsIdentical(moduleAfter);

            Assert.IsTrue(areModulesIdentical, "Modules should be equal.");
        }

        private void DeleteAllSceanriosPostfix([NotNull] ModelContainer container)
        {
            var scenarios = container.GetAll<ScenarioModel>();
            foreach (var scenario in scenarios)
            {
                var postfixStartIndex = scenario.Name.LastIndexOf('-');
                if (postfixStartIndex < 0)
                {
                    continue;
                }

                scenario.Name = scenario.Name.Substring(0, postfixStartIndex);
            }
        }
    }
} 
