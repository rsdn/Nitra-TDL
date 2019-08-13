using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Services;
using Tdl;
using Tdl2Json;

namespace Tdl.Transformator.Models.Products
{
    public sealed class ProductSetModel : ProductBaseModel
    {
        private readonly int _typeId;

        // ReSharper disable once NotNullMemberIsNotInitialized
        public ProductSetModel([NotNull]Product.SetSymbol symbol)
            : base(symbol, ModelType.ProductSet)
        {
            _typeId = symbol.ProductType.Id;
            Members = ((Product.Set)symbol.FirstDeclarationOrDefault).Products.Ref
                .Select(reference =>
                    new ReferenceModel<ProductBaseModel>(reference.Symbol.Id, reference.Location))
                .ToList();
        }

        [NotNull]
        public ProductTypeModel ProductType { get; set; }

        [NotNull, ItemNotNull]
        public List<ReferenceModel<ProductBaseModel>> Members { get; set; }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"product {Name} : {ProductType.Name} =");
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
            ProductType = modelContainer.Get<ProductTypeModel>(_typeId);
            ProductType.ProductSetRefs.Add(this);
            foreach (var member in Members)
            {
                member.Init(modelContainer);
                member.Model.ProductSetRefs.Add(this);
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
            var another = (ProductSetModel)other;

            return ProductType.IsIdentical(another.ProductType)
                   && Members.IsIdentical(another.Members);
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (ProductSetModel)model;

            clone.Members = Members.ToList();

            return clone;
        }
    }
}