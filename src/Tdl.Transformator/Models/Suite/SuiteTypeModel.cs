using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Models.Parameters;
using KL.TdlTransformator.Models.TypedReference;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Suite
{
    public sealed class SuiteTypeModel : SuiteRelatable
    {
        public SuiteTypeModel([NotNull]SuiteTypeSymbol symbol)
            : base(symbol, ModelType.SuiteType)
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

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.AppendLine($"suite type {Name}");
            builder.AppendLine("{");
            foreach (var parameter in ParameterValues)
            {
                builder.AppendLine($"{Constants.TabulationSymbol}{parameter.Type} {parameter.Name};");
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
        } 

        protected override bool IsIdenticCore(Model model)
        {
            var other = (SuiteTypeModel)model;

            return ParameterValues.IsIdentical(other.ParameterValues);
        }

        protected override object Clone([NotNull] object model)
        {
            var clone = (SuiteTypeModel)model;

            clone.ParameterValues = ParameterValues.Select(p => (FieldModel)p.Clone()).ToList();

            return clone;
        }
    }
} 