using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Tdl.Transformator.Models;
using Tdl.Transformator.Models.Modules;
using Tdl.Transformator.Models.Products;
using Tdl.Transformator.Models.Scenario;
using Tdl.Transformator.Models.Scenario.Actions;
using Tdl.Transformator.Models.Suite;
using Tdl.Transformator.Services;
using NLog;

namespace Tdl.Transformator.Processors
{
    public sealed class ScenarioProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();     

        public void RemoveUnused([ItemNotNull, NotNull] IEnumerable<ScenarioModel> scenarios)
        {
            var scenarioModels = scenarios
                .Where(scenario => scenario.IsAnyBackReferenceSet == false);

            Logger.Info($"removing unused scenarios, total: {scenarioModels.Count()}");

            foreach (var scenarioModel in scenarioModels)
            {
                Logger.Info($"unused scenario {scenarioModel.Name}, methods:");
                foreach (var action in scenarioModel.Actions)
                {
                    Logger.Trace($"action {action.Print()}");
                }

                var duplicates = scenarios
                    .Where(scenario =>
                        scenario.Actions.IsIdentical(scenarioModel.Actions)
                        && scenario.Id != scenarioModel.Id)
                    .ToArray();

                Logger.Trace($"other scenarios with same methods:{string.Join(", ", duplicates.Select(dup => dup.Name))}");
                Logger.Trace($"[tab]{scenarioModel.Name}\t{string.Join("; ", scenarioModel.Actions.Select(act => act.Print()))}\t{string.Join("; ", duplicates.Select(dup => dup.Name))}");
            }

            Logger.Info("removing finished");
        }

        public void ClearScenarioNames([ItemNotNull, NotNull] IEnumerable<ScenarioModel> scenarios)
        {
            var scenarioModels = scenarios.ToList();
            Logger.Info($"clearing scenario names: {scenarioModels.Count()}");
            var regex = new Regex("(.*?)-[0-9]*");
            var processed = new List<int>();
            var counter = 1;
            foreach (var scenarioModel in scenarioModels)
            {
                if (processed.Any(id => id == scenarioModel.Id))
                {
                    counter++;
                    continue;
                }

                Logger.Info($"Checking scenario {counter++}/{scenarioModels.Count()}: {scenarioModel.Name}");

                processed.Add(scenarioModel.Id);

                var match = regex.Match(scenarioModel.Name);
                if (match.Success)
                {
                    var name = match.Groups[1].Value;
                    var comparisonName = name.ToLower();
                    var scenariosWithSameName = scenarioModels
                        .Where(model => model.Id != scenarioModel.Id
                            && (model.Name.ToLower().Equals(comparisonName)
                                 || (model.Name.ToLower().Contains(comparisonName) && regex.Match(model.Name).Groups[1].Value.ToLower().Equals(comparisonName))));

                    if (scenariosWithSameName.Any())
                    {
                        Logger.Trace($"scenario with same name {name}: {string.Join(", ", scenariosWithSameName.Select(scenario => scenario.Name))}");
                        Logger.Trace($"[tab] {scenarioModel.Name}\t{name}\t"
                                     + $"{string.Join("; ", scenariosWithSameName.Select(scenario => scenario.Name))}\t"
                                      + $"{string.Join("; ", scenarioModel.Actions.Select(act => act.Print()))}");

                        processed.AddRange(scenariosWithSameName.Select(scenario => scenario.Id));
                        Logger.Trace(scenarioModel.Print());
                        foreach (var dup in scenariosWithSameName)
                        {
                            Logger.Trace(dup.Print());
                        }

                        continue;
                    }

                    Logger.Trace($"no other  scenario has name {scenarioModel.Name}, replacing it with {name}");
                    scenarioModel.Name = name;
                }
            }

            Logger.Info("clearing scenario names");
        }

        public void ReplaceDuplicates([ItemNotNull, NotNull] IEnumerable<ScenarioModel> scenarios)
        {
            ReplaceDuplicates(scenarios,
                (left, right) => left.IsIdentical(right) && left.Id != right.Id);
        }

        public void ReplaceDuplicates([ItemNotNull, NotNull] IEnumerable<ScenarioSetModel> scenarios)
        {
            ReplaceDuplicates(scenarios,
                (left, right) => left.IsIdentical(right) && left.Id != right.Id);
        }

        public void ReplaceDuplicatesWIthDifferentScenarios([ItemNotNull, NotNull] IEnumerable<ScenarioModel> scenarios)
        {
            const string uniqId = "UniqueId";

            ReplaceDuplicates(scenarios,
                (ScenarioModel left, ScenarioModel right) =>
                {
                    return right is ScenarioModel another
                           && left.Definitions.Where(def => def.Name != uniqId)
                               .SequenceEqual(another.Definitions.Where(def => def.Name != uniqId))
                           && left.Deployment.IsIdentical(another.Deployment)
                           && left.Actions.IsIdentical(another.Actions)
                           && left.Id != right.Id;
                });
        }

        public void SimplifyEnvironments(
            [ItemNotNull, NotNull] IEnumerable<ScenarioModel> scenarios,
            [ItemNotNull, NotNull] IEnumerable<ProductSetModel> productsSets)
        {
            var scenarioModels = scenarios
                .Where(scenario => scenario.Environments.Count > 1)
                .ToArray();

            Logger.Info($"merging duplicated environments {scenarioModels.Length}");

            var environmentsProcessor = new EnvironmentProcessor();
            var counter = 1;
            foreach (var scenarioModel in scenarioModels)
            {
                Logger.Info($"checking {counter++}/{scenarioModels.Count()}:{scenarioModel.Name}");
                Logger.Trace($"{scenarioModel.Name} old env: {string.Join(", ", scenarioModel.Environments.Select(env => env.Print()))}");
                var mergedEnvironments
                    = environmentsProcessor.MergeEnvironments(scenarioModel.Environments, productsSets);
                scenarioModel.Environments.Clear();
                scenarioModel.Environments.AddRange(mergedEnvironments);
                Logger.Trace($"{scenarioModel.Name} new env: {string.Join(", ", scenarioModel.Environments.Select(env => env.Print()))}");
            }

            Logger.Info("merging duplicated environments finished");
        }

        public void ReplaceDuplicates(
            [ItemNotNull, NotNull] IEnumerable<ScenarioBaseModel> scenarios,
            [NotNull] Func<ScenarioBaseModel, ScenarioBaseModel, bool> equalityPredicate)
        {
            Logger.Info($"removing duplicated scenarios total {scenarios.Count()}");
            var processed = new List<int>();
            var counter = 1;
            foreach (var scenarioModel in scenarios)
            {
                counter++;
                if (processed.Any(id => id == scenarioModel.Id))
                {
                    continue;
                }

                Logger.Info($"checking {counter}/{scenarios.Count()}:{scenarioModel.Name}");
                processed.Add(scenarioModel.Id);
                var duplicates = scenarios
                    .Where(scenario =>
                      equalityPredicate(scenarioModel, scenario))
                    .ToList();

                if (duplicates.Any())
                {
                    processed.AddRange(duplicates.Select(scenario => scenario.Id));
                    Logger.Info($"dupes of {scenarioModel.Name} : {string.Join(", ", duplicates.Select(dup => dup.Name))}");

                    Logger.Trace(scenarioModel.Print());
                    foreach (var dup in duplicates)
                    {
                        Logger.Trace(dup.Print());
                    }

                    ReplaceDuplicateScenarios(scenarioModel, duplicates);
                }
            }

            Logger.Info("removing duplicated finished");
        }

        public void ReplaceDuplicates(
            [ItemNotNull, NotNull] IEnumerable<ScenarioModel> scenarios,
            [NotNull] Func<ScenarioModel, ScenarioModel, bool> equalityPredicate)
        {
            Logger.Info($"removing duplicated scenarios total {scenarios.Count()}");
            var processed = new List<int>();
            var environmentsProcessor = new EnvironmentProcessor();
            var counter = 1;
            foreach (var scenarioModel in scenarios)
            {
                counter++;
                if (processed.Any(id => id == scenarioModel.Id))
                {
                    continue;
                }

                Logger.Info($"checking {counter}/{scenarios.Count()}:{scenarioModel.Name}");
                processed.Add(scenarioModel.Id);
                var duplicates = scenarios
                    .Where(scenario =>
                      equalityPredicate(scenarioModel, scenario))
                    .ToList();

                if (duplicates.Any())
                {
                    var mergedEnvironments = environmentsProcessor.ConcatenateEnvironments(
                        duplicates
                            .SelectMany(dup => dup.Environments)
                            .Concat(scenarioModel.Environments));
                    scenarioModel.Environments.Clear();
                    scenarioModel.Environments.AddRange(mergedEnvironments);

                    processed.AddRange(duplicates.Select(scenario => scenario.Id));
                    Logger.Info($"dupes of {scenarioModel.Name} : {string.Join(", ", duplicates.Select(dup => dup.Name))}");

                    Logger.Trace(scenarioModel.Print());
                    foreach (var dup in duplicates)
                    {
                        Logger.Trace(dup.Print());
                    }

                    ReplaceDuplicateScenarios(scenarioModel, duplicates);
                }
            }

            Logger.Info("removing duplicated finished");
        }

        [NotNull, ItemNotNull]
        public List<ScenarioModel> Ungrouping([NotNull] ModelContainer container, [CanBeNull] Func<ScenarioModel, bool> isIdoneous)
            => Ungrouping(container.GetAll<ScenarioModel>(), container, isIdoneous);

        [NotNull, ItemNotNull]
        public List<ScenarioModel> Ungrouping(
           [NotNull, ItemNotNull] IEnumerable<ScenarioModel> scenarios,
           [NotNull] ModelContainer container,
           [CanBeNull] Func<ScenarioModel, bool> isIdoneous)
        {
            bool hasSequence(ScenarioModel model) => model.HasSequence;

            var groupedScenarios = scenarios.Where(isIdoneous ?? hasSequence);

            var result = new List<ScenarioModel>();
            foreach (var scenario in groupedScenarios)
            {
                var methods = scenario.Actions.OfType<MethodActionModel>();
                if (methods.Count() > 1)
                {
                    foreach (var method in methods)
                    {
                        var newScenario = CreateScenarioWithMethods(scenario, new[] { method }, container);
                        result.Add(newScenario);
                    }

                    Remove(scenario);
                    container.Remove(scenario.Id);
                }
            }

            return result;
        }

        [NotNull, ItemNotNull]
        public List<ScenarioModel> UngroupingByParameter<TParameterType>(
            [NotNull] ModelContainer container,
            [NotNull] string parameterName,
            [NotNull] Func<IEnumerable<MethodActionModel>, IEnumerable<IGrouping<TParameterType, MethodActionModel>>> groupMethod)
        {
            var scenarios = container.GetAll<ScenarioModel>().Where(s => s.HasSequence);
            var newScenarios = new List<ScenarioModel>(); 

            var names = new HashSet<string>(container.GetAll<ScenarioModel>().Select(s => s.Name));
            
            foreach (var scenario in scenarios)
            {
                var scenarioParameterValue = scenario.Definitions.Where(d => d.Name == parameterName).Single().Expression.ToString();

                var methods = scenario.Actions.OfType<MethodActionModel>();
                var groups = groupMethod(methods);

                if (groups.All(g => g.Key.ToString() == scenarioParameterValue.Trim('"')))
                {
                    continue;
                }

                if (groups.Count() == 1)
                {
                    UpdateScenarioDefinitionModel(scenario, parameterName, groups.First().Key);
                    continue;
                }

                names.Remove(scenario.Name);

                foreach (var group in groups)
                {
                    var newScenario = CreateScenarioWithMethods(scenario, group, container);
                    PrepareName(newScenario, names);
                    UpdateScenarioDefinitionModel(newScenario, parameterName, group.Key);
                    newScenarios.Add(newScenario); 
                }

                Remove(scenario);
                container.Remove(scenario.Id); 
            } 

            return newScenarios;
        }

        [NotNull, ItemNotNull]
        public List<ScenarioModel> UngroupingByQcId([NotNull] ModelContainer container)
        {
            const string qcIdDef = "QcId";
            const string tfsIdDef = "TfsId";

            IEnumerable<IGrouping<int, MethodActionModel>> groupMethod(IEnumerable<MethodActionModel> methods)
            {
                var result = methods.GroupBy(m => m.Method.MethodSymbol.CustomAttributes
                    .Where(attr => attr.Arguments?.Count > 1 && attr.Arguments[0].ToString() == qcIdDef)
                    .Select(a => Convert.ToInt32(a.Arguments[1].ToString()))
                    .FirstOrDefault());

                return result.Count() == 1 && result.First().Key == 0
                    ? Enumerable.Empty<IGrouping<int, MethodActionModel>>()
                    : result;
            }

            return UngroupingByParameter(container, tfsIdDef, groupMethod);
        }

        [NotNull, ItemNotNull]
        public List<ScenarioModel> UngroupingByArea([NotNull] ModelContainer container)
        {
            const string areaDef = "Area";

            IEnumerable<IGrouping<string, MethodActionModel>> groupMethod(IEnumerable<MethodActionModel> methods) =>
                methods.GroupBy(m => GetAreaByNamespace(m));

            return UngroupingByParameter(container, areaDef, groupMethod);
        }

        public static void ReplaceDuplicateScenarios([NotNull] ScenarioBaseModel replacement, [ItemNotNull, NotNull] IEnumerable<ScenarioBaseModel> duplicates)
        {
            foreach (var dup in duplicates)
            {
                ReplaceDuplicateScenario(replacement, dup);
            }
        }

        [NotNull]
        private string GetAreaByNamespace([NotNull] MethodActionModel method)
        {
            var slicedName = method.FullName.Split('.');
            if (slicedName.Length < 3)
            {
                throw new InvalidOperationException("method full name should contain at least 3 parts");
            }

            return slicedName[slicedName.Length - 3];
        }

        [NotNull]
        private ScenarioModel CreateScenarioWithMethods(
            [NotNull] ScenarioModel baseScenario,
            [NotNull, ItemNotNull] IEnumerable<MethodActionModel> methods,
            [NotNull] ModelContainer container)
        {
            const string uniqueIdDef = "UniqueId";

            var precedingActionTypes = new[]
            {
                typeof(BarrierActionModel),
                typeof(RebootActionModel)
            };

            var newScenario = (ScenarioModel)baseScenario.Clone();
            newScenario.Id = container.NextId();
            newScenario.Name = methods.First().Method.MethodSymbol.Name;
            newScenario.Actions = new List<BaseActionModel>();

            var originalActions = baseScenario.Actions;
            foreach (var method in methods)
            {
                var index = originalActions.FindIndex(m => m == method);
                var i = index;
                while (--i >= 0 && precedingActionTypes.Contains(originalActions[i].GetType()))
                {
                }

                while (++i < index)
                {
                    newScenario.Actions.Add(originalActions[i]);
                }

                newScenario.Actions.Add(method);

                if (index < originalActions.Count - 2
                     && originalActions[index + 1].GetType() == typeof(WaitForRebootActionModel))
                {
                    newScenario.Actions.Add(originalActions[index + 1]);
                }
            }

            UpdateScenarioDefinitionModel(newScenario, uniqueIdDef, Guid.NewGuid().ToString());
            newScenario.Init(container);
            UpdateReference(newScenario);

            container.Add(newScenario);

            return newScenario;
        }

        private void UpdateScenarioDefinitionModel<TValue>(
            [NotNull] ScenarioModel scenario,
            [NotNull] string definitionName,
            [NotNull] TValue value)
        {
            var index = scenario.Definitions.FindIndex(d => d.Name == definitionName);
            if (index >= 0)
            {
                var expression = Models.Expressions.Expression.Create(value);
                scenario.Definitions[index].Expression = expression;
            }
        }

        private void UpdateReference([NotNull] ScenarioBaseModel scenario)
        {
            foreach (var set in scenario.ScenarioSetRefs.References)
            {
                set.Members.Add(new ReferenceModel<ScenarioBaseModel>(scenario));
            }

            foreach (var suite in scenario.SuiteRefs.References)
            {
                suite.Statements.Add(scenario);
            }

            scenario.Module.Items.Add(scenario);
        }

        private void Remove([NotNull] ScenarioBaseModel scenario)
        {
            foreach (var set in scenario.ScenarioSetRefs.References)
            {
                var index = set.Members.FindIndex(m => m.Model.Id == scenario.Id);
                if (index >= 0)
                {
                    set.Members.RemoveAt(index);
                }
            }

            foreach (var suite in scenario.SuiteRefs.References)
            {
                var index = suite.Statements.FindIndex(s => s.Id == scenario.Id);
                if (index >= 0)
                {
                    suite.Statements.RemoveAt(index);
                }
            }

            if (scenario is ScenarioModel scenarioModel)
            {
                scenarioModel.Deployment.Model.ScenarioRefs.Remove(scenarioModel);
            }

            RemoveFromModule(scenario.Module, scenario);
        }

        private void PrepareName([NotNull] ScenarioModel scenario, [NotNull, ItemNotNull] HashSet<string> names)
        {
            if (names.Contains(scenario.Name))
            {
                scenario.Name = scenario.Name + $"-{scenario.Id}";
                return;
            }

            names.Add(scenario.Name);
        }

        private static void ReplaceDuplicateScenario(
            [NotNull] ScenarioBaseModel replacement,
            [NotNull] ScenarioBaseModel replaced)
        {
            foreach (var set in replaced.ScenarioSetRefs.References)
            {
                ReplaceScenarioInSet(set, replaced, replacement);
            }

            foreach (var suite in replaced.SuiteRefs.References)
            {
                ReplaceInSuite(suite, replaced, replacement);
            }

            RemoveFromModule(replaced.Module, replaced);
        }

        private static void RemoveFromModule(
            [NotNull] ModuleModel module,
            [NotNull] ScenarioBaseModel replaced)
        {
            // we need to remove scenario from module, because it is already defined in another module
            var oldIndex = module.Items.FindIndex(ind => ind.Id == replaced.Id);
            if (oldIndex >= 0)
            {
                Logger.Info($"removing {replaced.Name} in module {module.Path}");
                module.Items.RemoveAt(oldIndex);
            }
        }

        private static void ReplaceInSuite(
            [NotNull] SuiteDefModel suite,
            [NotNull] ScenarioBaseModel replaced,
            [NotNull] ScenarioBaseModel replacement)
        {
            var oldIndex = suite.Statements.FindIndex(ind => ind.Id == replaced.Id);
            if (oldIndex >= 0)
            {
                Logger.Info($"replacing {replaced.Name} in suite {suite.Name} to {replacement.Name}");
                suite.Statements[oldIndex] = replacement;
            }
        }

        private static void ReplaceScenarioInSet(
            [NotNull] ScenarioSetModel set,
            [NotNull] ScenarioBaseModel replaced,
            [NotNull] ScenarioBaseModel replacement)
        {
            var oldIndex = set.Members.FindIndex(ind => ind.Model.Id == replaced.Id);
            if (oldIndex >= 0)
            {
                var replacementIsPresent = set.Members.FindIndex(ind => ind.Model.Id == replacement.Id) >= 0;
                if (replacementIsPresent)
                {
                    Logger.Info($"removing {replaced.Name} from set {set.Name} because original already present");
                    set.Members.RemoveAt(oldIndex);
                }
                else
                {
                    Logger.Info($"replacing {replaced.Name} in set {set.Name} to {replacement.Name}");
                    set.Members[oldIndex] = new ReferenceModel<ScenarioBaseModel>(replacement);
                }
            }
        } 
    }
}
