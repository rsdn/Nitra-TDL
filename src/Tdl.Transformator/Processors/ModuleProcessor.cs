using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using KL.TdlTransformator.Models.Modules;
using KL.TdlTransformator.Models.Scenario;
using NLog;

namespace KL.TdlTransformator.Processors
{
    public sealed class ModuleProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void SortModuleMembers([ItemNotNull, NotNull] IEnumerable<ModuleModel> modules)
        {
            //currently only scenarios and scenario sets are separeated.
            
            Logger.Info($"sorting members total modules: {modules.Count()}");
            foreach (var module in modules)
            {
                var localCopy = module.Items.ToList();
                Logger.Info($"sorting module: {module.Path}");
                Logger.Trace($"module {module.Path} old order: {string.Join(Environment.NewLine, localCopy.Select(item => $"{item.ModelType}:{item.Name}"))}");

                var scenarios = localCopy.OfType<ScenarioModel>().OrderBy(set => set.Name).ToList();
                scenarios.ForEach(scenario => localCopy.Remove(scenario));

                var scenarioSets = localCopy.OfType<ScenarioSetModel>().OrderBy(scenario => scenario.Name).ToList();
                scenarioSets.ForEach(set => localCopy.Remove(set));

                var otherItems = localCopy.OrderBy(item => item.Name).ToList();
                otherItems.ForEach(item => localCopy.Remove(item));

                if (localCopy.Any())
                {
                    Logger.Error("some items were left in local copy");
                }

                localCopy.AddRange(otherItems);
                localCopy.AddRange(scenarioSets);
                localCopy.AddRange(scenarios);
                Logger.Trace($"module {module.Path} new order: {string.Join(Environment.NewLine, localCopy.Select(item => $"{item.ModelType}:{item.Name}"))}");

                module.Items.Clear();
                module.Items.AddRange(localCopy);
            }

            Logger.Info("sorting finished");
        }
    }
}
