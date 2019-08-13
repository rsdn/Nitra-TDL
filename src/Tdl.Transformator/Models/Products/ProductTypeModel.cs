using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Parameters;
using Tdl.Transformator.Models.TypedReference;
using Tdl.Transformator.Services;
using Tdl;

namespace Tdl.Transformator.Models.Products
{
    public sealed class ProductTypeModel : Model
    {
        public ProductTypeModel([NotNull]ProductTypeSymbol symbol)
            : base(symbol, ModelType.ProductType)
        {
            ParameterValues = new List<FieldModel>();
            foreach (var declarationSymbol in symbol.MemberTable.GetAllSymbols())
            {
                if (declarationSymbol is FieldSymbol fieldSymbol)
                {
                    ParameterValues.Add(new FieldModel(fieldSymbol));
                }
                else
                {
                    Logger.Error(
                        $"{declarationSymbol} is not {typeof(FieldSymbol)}, but {declarationSymbol.GetType()}");
                }
            }
        }

        [NotNull, ItemNotNull]
        public List<FieldModel> ParameterValues { get; set; }

        [NotNull]
        public BackReference<ProductSetModel> ProductSetRefs { get; private set; } = new BackReference<ProductSetModel>();

        [NotNull]
        public BackReference<ProductModel> ProductRefs { get; private set; } = new BackReference<ProductModel>();

        public override bool HasBackReference => true;

        public override bool IsAnyBackReferenceSet => ProductSetRefs.Any() || ProductRefs.Any();

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"product type {Name}");
            builder.AppendLine("{");
            foreach (var parameter in ParameterValues)
            {
                builder.AppendLine($"{Constants.TabulationSymbol}{parameter.Print()};");
            }

            builder.Append("}");

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (ProductTypeModel)other;

            return ParameterValues.IsIdentical(another.ParameterValues);
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (ProductTypeModel)model;

            clone.ParameterValues = ParameterValues.Select(p => (FieldModel)p.Clone()).ToList();

            return clone;
        }
    }
} 