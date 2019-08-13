using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Parameters;
using Tdl.Transformator.Services;
using Tdl;
using Tdl2Json;

namespace Tdl.Transformator.Models.Products
{
    public sealed class ProductModel : ProductBaseModel
    {
        private readonly int _typeId;

        // ReSharper disable once NotNullMemberIsNotInitialized
        public ProductModel([NotNull]Product.DefSymbol symbol)
            : base(symbol, ModelType.Product)
        {
            _typeId = symbol.ProductType.Id;
            Definitions = DefinitionGenerator.GetDefinitions(symbol.Defs,
                ((Product.Def)symbol.FirstDeclarationOrDefault).Definitions);
        }

        [NotNull]
        public ProductTypeModel Type { get; set; }

        [NotNull, ItemNotNull]
        public List<DefinitionModel> Definitions { get; set; }

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"product {Name} : {Type.Name}");
            builder.AppendLine("{");
            foreach (var def in Definitions)
            {
                builder.AppendLine($"{Constants.TabulationSymbol}{def.Print()};");
            }

            builder.Append("}");
            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
            Type = modelContainer.Get<ProductTypeModel>(_typeId);
            Type.ProductRefs.Add(this);
        }

        public override IEnumerable<CommentBlock> FindCommentsForMembers(CommentBag commentBag)
        {
            var results = new List<CommentBlock>();
            results.AddRange(base.FindCommentsForMembers(commentBag));
            results.AddRange(Definitions.SelectMany(model =>
                SymbolConverter.SearchComments(commentBag, model.Comments)));
            return results;
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (ProductModel)other;

            return Type.IsIdentical(another.Type)
                   && Definitions.SequenceEqual(another.Definitions);
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (ProductModel)model;

            clone.Definitions = Definitions.Select(d => (DefinitionModel)d.Clone()).ToList();

            return clone;
        }
    }
}