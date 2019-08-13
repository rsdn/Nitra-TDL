using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Parameters;
using Tdl.Transformator.Services;
using Tdl;
using Tdl2Json;

namespace Tdl.Transformator.Models.Deployments
{
    public sealed class DeploymentScriptModel : CurryingApplicableDeployment
    {
        public DeploymentScriptModel([NotNull] Deployment.ScriptSymbol symbol)
            : base(symbol, ModelType.DeploymentScript)
        {
            Parameters = new List<ParameterModel>();
            Parameters.AddRange(symbol.Parameters.Select(p => new ParameterModel(p)));

            Definitions = DefinitionGenerator.GetDefinitions(symbol.Defs,
                ((Deployment.Script)symbol.FirstDeclarationOrDefault).Definitions);

            Options = new List<ValueModel>();
            foreach (var option in ((Deployment.Script)symbol.FirstDeclarationOrDefault).Options)
            {
                switch (option)
                {
                    case DeploymentOption.Success success:
                        Options.Add(new ValueModel(success.Value, TypesOfValue.Success, success.Location));
                        break;
                    case DeploymentOption.ForReboot reboot:
                        Options.Add(new ValueModel(reboot.Value, TypesOfValue.Reboot, reboot.Location));
                        break;
                    case DeploymentOption.Timeout timeout:
                        Options.Add(new ValueModel(symbol.Timeout.Value, TypesOfValue.Timeout, timeout.Location));
                        break;
                    default:
                        Logger.Error($"unsupported option type: {option}");
                        break;
                }
            }

            ScriptReference = ScriptReference.Create(symbol);
        }

        [NotNull]
        public ScriptReference ScriptReference { get; set; }

        [NotNull, ItemNotNull]
        public List<ValueModel> Options { get; set; }

        [NotNull, ItemNotNull]
        public List<DefinitionModel> Definitions { get; set; }

        [NotNull, ItemNotNull]
        public List<ParameterModel> Parameters { get; set; }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"deployment \"{Name}\"(");
            if (Parameters.Count == 0)
            {
                builder.AppendLine($"{Constants.TabulationSymbol})");
            }
            else
            {
                for (var i = 0; i < Parameters.Count; i++)
                {
                    var endLineSymbol = i == Parameters.Count - 1 ? ")" : ",";
                    builder.AppendLine($"{Constants.TabulationSymbol}{Parameters[i].Print()}{endLineSymbol}");
                }
            }

            builder.AppendLine($"{Constants.TabulationSymbol}{ScriptReference.Print()}");
            builder.AppendLine("{");
            foreach (var def in Definitions)
            {
                builder.AppendLine($"{Constants.TabulationSymbol}{def.Print()};");
            }

            foreach (var valueModel in Options)
            {
                builder.AppendLine($"{valueModel};");
            }

            builder.Append("}");
            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
        }

        public override IEnumerable<CommentBlock> FindCommentsForMembers(CommentBag commentBag)
        {
            var results = new List<CommentBlock>();
            results.AddRange(base.FindCommentsForMembers(commentBag));
            results.AddRange(Parameters.SelectMany(model =>
                SymbolConverter.SearchComments(commentBag, model.Comments)));
            results.AddRange(Options.SelectMany(model =>
                SymbolConverter.SearchComments(commentBag, model.Comments)));
            results.AddRange(Definitions.SelectMany(model =>
                SymbolConverter.SearchComments(commentBag, model.Comments)));
            results.AddRange(ScriptReference.FindCommentsForMembers(commentBag));
            return results;
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (DeploymentScriptModel)other;

            return ScriptReference.IsIdentical(another.ScriptReference)
                   && Definitions.SequenceEqual(another.Definitions)
                   && Parameters.IsIdentical(another.Parameters)
                   && Options.IsIdentical(another.Options);
        }

        protected override object Clone(object model)
        {
            model = base.Clone(model);

            var clone = (DeploymentScriptModel)model;

            clone.Options = Options.Select(o => (ValueModel)o.Clone()).ToList();
            clone.Parameters = Parameters.Select(p => (ParameterModel)p.Clone()).ToList();
            clone.Definitions = Definitions.Select(d => (DefinitionModel)d.Clone()).ToList();

            return clone;
        }
    }
} 
