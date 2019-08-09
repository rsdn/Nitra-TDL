using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Services;
using Tdl2Json;

namespace KL.TdlTransformator.Models.Scenario
{
    public sealed class ScenarioSetModel : ScenarioBaseModel
    {
        public ScenarioSetModel([NotNull] Tdl.Scenario.SetSymbol symbol)
            : base(symbol, ModelType.ScenarioSet)
        {
            Members = ((Tdl.Scenario.Set)symbol.FirstDeclarationOrDefault).Scenarios.Ref.Select(reference =>
                new ReferenceModel<ScenarioBaseModel>(reference.Symbol.Id, reference.Location)).ToList();
            IsMultiMachine = symbol.IsMultiMachine;
        }

        public ScenarioSetModel(
            ModelType modelType, 
            int id, 
            Nitra.Location? location, 
            [NotNull] string name, 
            [NotNull] string path,
            [NotNull, ItemNotNull] List<ReferenceModel<ScenarioBaseModel>> members) 
            : base(modelType, id, location, name, path)
         => Members = members; 

        [ItemNotNull, NotNull]
        public List<ReferenceModel<ScenarioBaseModel>> Members { get; private set; }

        public bool IsMultiMachine { get; set; }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"{(IsMultiMachine ? "parallel " : string.Empty)}scenario {Name} =");
            
            for (var i = 0; i < Members.Count; i++)
            {
                builder.Append($"{Members[i].Print()}");
                if (i != Members.Count - 1)
                {
                    builder.AppendLine(",");
                }
            }

            builder.Append(";");

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
            foreach (var scenario in Members)
            {
                scenario.Init(modelContainer);
                scenario.Model.ScenarioSetRefs.Add(this);
            }
        }

        public override IEnumerable<CommentBlock> FindCommentsForMembers(CommentBag commentBag)
        {
            var results = new List<CommentBlock>();
            results.AddRange(base.FindCommentsForMembers(commentBag));
            foreach (var member in Members)
            {
                results.AddRange(SymbolConverter.SearchComments(commentBag, member.Comments));
            }

            return results;
        }

        protected override bool IsIdenticCore(Model model)
        {
            var another = (ScenarioSetModel)model;

            return Members.IsIdentical(another.Members);
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (ScenarioSetModel)base.Clone(model);

            clone.Members = Members.ToList();

            return clone;
        }
    }
} 