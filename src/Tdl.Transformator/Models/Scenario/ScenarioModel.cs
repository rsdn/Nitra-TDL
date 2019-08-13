using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Deployments;
using Tdl.Transformator.Models.Parameters;
using Tdl.Transformator.Models.Scenario.Actions;
using Tdl.Transformator.Services;
using Tdl2Json;

namespace Tdl.Transformator.Models.Scenario
{
    public sealed class ScenarioModel : ScenarioBaseModel
    {
        public ScenarioModel([NotNull]Tdl.Scenario.DefSymbol symbol)
            : base(symbol, ModelType.Scenario)
        {
            Definitions = DefinitionGenerator.GetDefinitions(symbol.Defs,
                ((Tdl.Scenario.Def)symbol.FirstDeclarationOrDefault).Definitions);

            var reference = ((Tdl.Scenario.Def)symbol.FirstDeclarationOrDefault).DeploymentRef;
            Deployment = new ReferenceModel<DeploymentBaseModel>(reference.Symbol.Id, reference.Location);

            Environments = new List<ScenarioEnvironmentModel>();
            Environments.AddRange(
                symbol.Environments.Select(env =>
                    new ScenarioEnvironmentModel(env)));
            Actions = new List<BaseActionModel>();
            ParseMethods(symbol.Actions);
        }

        public ScenarioModel(
            ModelType modelType,
            int id,
            Nitra.Location? location,
            [NotNull] string name,
            [NotNull] string path,
            [NotNull, ItemNotNull] List<DefinitionModel> definitions,
            [NotNull, ItemNotNull] ReferenceModel<DeploymentBaseModel> deployments,
            [NotNull, ItemNotNull] List<ScenarioEnvironmentModel> environments,
            [NotNull, ItemNotNull] List<BaseActionModel> actions)
            : base(modelType, id, location, name, path)
        {
            Definitions = definitions;
            Deployment = deployments;
            Environments = environments;
            Actions = actions;
        } 

        [NotNull, ItemNotNull]
        public List<DefinitionModel> Definitions { get; set; }

        [NotNull, ItemNotNull]
        public List<ScenarioEnvironmentModel> Environments { get; set; }

        [NotNull]
        public ReferenceModel<DeploymentBaseModel> Deployment { get; set; }

        [NotNull, ItemNotNull]
        public List<BaseActionModel> Actions { get; set; }

        public bool HasSequence => Actions.Count > 1;

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"scenario {Name}");
            builder.AppendLine("{");
            Deployment.Comments?.AppendComments(builder);
            builder.AppendLine($"{Constants.TabulationSymbol}deployment {Deployment.Print(PrintParameters.NameOnly)};");

            var environments = string.Join(", ", Environments.Select(env => env.Print()));
            builder.AppendLine($"{Constants.TabulationSymbol}environments {environments};");
            if (Actions.Count > 1)
            {
                builder.AppendLine($"{Constants.TabulationSymbol}sequence");
                builder.AppendLine($"{Constants.TabulationSymbol}{{");

                foreach (var baseActionModel in Actions)
                {
                    builder.AppendLine($"{Constants.TabulationSymbol}{Constants.TabulationSymbol}{baseActionModel.Print()}");
                }

                builder.AppendLine($"{Constants.TabulationSymbol}}}");
            }
            else
            {
                builder.AppendLine($"{Constants.TabulationSymbol}{Actions.First().Print()}");
            }

            foreach (var definition in Definitions)
            {
                builder.AppendLine($"{Constants.TabulationSymbol}{definition.Print()};");
            }

            builder.Append("}");

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
            Deployment.Init(modelContainer);
            Deployment.Model.ScenarioRefs.Add(this);

            foreach (var barrierActionModel in Actions.OfType<BarrierActionModel>())
            {
                barrierActionModel.Init(modelContainer);
            }

            foreach (var scenarioEnvironmentModel in Environments)
            {
                scenarioEnvironmentModel.Init(modelContainer);
            }
        }

        public override IEnumerable<CommentBlock> FindCommentsForMembers(CommentBag commentBag)
        {
            var results = new List<CommentBlock>();
            results.AddRange(base.FindCommentsForMembers(commentBag));
            results.AddRange(Actions.SelectMany(model =>
                SymbolConverter.SearchComments(commentBag, model.Comments)));
            results.AddRange(Definitions.SelectMany(model =>
                SymbolConverter.SearchComments(commentBag, model.Comments)));
            results.AddRange(SymbolConverter.SearchComments(commentBag, Deployment.Comments));
            return results;
        }

        protected override bool IsIdenticCore(Model model)
        {
            const string uniqId = "UniqueId";

            return model is ScenarioModel another
                   && Definitions.Where(def => def.Name != uniqId)
                       .SequenceEqual(another.Definitions.Where(def => def.Name != uniqId))
                   && Environments.IsIdentical(another.Environments)
                   && Deployment.IsIdentical(another.Deployment)
                   && Actions.IsIdentical(another.Actions);
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (ScenarioModel)base.Clone(model);

            clone.Definitions = Definitions.Select(d => (DefinitionModel)d.Clone()).ToList();
            clone.Environments = Environments.ToList();
            clone.Actions = Actions.ToList();

            return clone;
        }

        private void ParseMethods([ItemNotNull]ImmutableArray<Tdl.ScenarioAction> actions)
        {
            foreach (var scenarioAction in actions)
            {
                switch (scenarioAction)
                {
                    case Tdl.ScenarioAction.Method method:
                        Actions.Add(new MethodActionModel(method, method.Location));
                        break;
                    case Tdl.ScenarioAction.Barrier barrier:
                        Actions.Add(new BarrierActionModel(barrier, barrier.Location));
                        break;
                    case Tdl.ScenarioAction.Config method:
                        Logger.Error("config is not supported");
                        break;
                    case Tdl.ScenarioAction.Reboot reboot:
                        Actions.Add(new RebootActionModel(reboot, reboot.Location));
                        break;
                    case Tdl.ScenarioAction.WaitForReboot reboot:
                        Actions.Add(new WaitForRebootActionModel(reboot, reboot.Location));
                        break;
                    default:
                        Logger.Error("Unknown scenario action: " + scenarioAction.GetType());
                        break;
                }
            }
        }        
    }
}