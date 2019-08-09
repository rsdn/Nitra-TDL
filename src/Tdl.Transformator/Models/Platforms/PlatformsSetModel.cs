using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Services;
using Tdl;
using Tdl2Json;

namespace KL.TdlTransformator.Models.Platforms
{
    public sealed class PlatformsSetModel : PlatformBase
    {
        public PlatformsSetModel([NotNull]Platform.SetSymbol symbol)
            : base(symbol, ModelType.PlatformSet)
        {
            Members = ((Platform.Set)symbol.FirstDeclarationOrDefault).Platforms.Ref
                .Select(reference =>
                    new ReferenceModel<PlatformBase>(reference.Symbol.Id, reference.Location))
                .ToList();
        }

        [NotNull, ItemNotNull]
        public List<ReferenceModel<PlatformBase>> Members { get; set; }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"platform {Name} =");
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
            foreach (var member in Members)
            {
                member.Init(modelContainer);
                member.Model.PlatfromSetRefs.Add(this);
            }
        }

        public override IEnumerable<CommentBlock> FindCommentsForMembers(CommentBag commentBag)
        {
            var results = new List<CommentBlock>();
            results.AddRange(base.FindCommentsForMembers(commentBag));
            results.AddRange(Members.SelectMany(model =>
                SymbolConverter.SearchComments(commentBag, model.Comments)));

            return results;
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (PlatformsSetModel)other;

            return Members.IsIdentical(another.Members);
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (PlatformsSetModel)model;

            clone.Members = Members.ToList();

            return clone;
        }
    }
} 