using System;
using System.Text;
using DotNet;
using JetBrains.Annotations;
using KL.TdlTransformator.Services;

namespace KL.TdlTransformator.Models.Parameters
{
    public sealed class ParameterModel : Model
    {
        public ParameterModel([NotNull]FormalParameterSymbol symbol) 
            : base(symbol, ModelType.Parameter)
        {
            switch (symbol.Default)
            {
                case DefaultValue.String sValue:
                    Value = sValue.Value;
                    break;
                case DefaultValue.Bool bValue:
                    Value = bValue.Value.ToString().ToLower();
                    break;
                case DefaultValue.Number nValue:
                    Value = nValue.Value.ToString().ToLower();
                    break;
                case DefaultValue.None _:
                    Value = null;
                    break;
                default: throw new Exception("script parameter type is not supported");
            }

            Type = symbol.Type.Name;
        }

        [CanBeNull]
        public string Value { get; set; }

        [NotNull]
        public string Type { get; set; }

        public override bool HasBackReference => false;

        public override string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);

            builder.Append($"{Type} {Name}");

            if (Value != null)
            {
                if (Type == "string")
                {
                    builder.Append($" = {Value.Print()}");
                }
                else
                {
                    builder.Append($" = {Value}");
                }
            }

            return builder.ToString();
        }

        public override void Init(ModelContainer modelContainer)
        {
        }

        protected override bool IsIdenticCore(Model other)
        {
            var another = (ParameterModel)other;

            return Name == another.Name
                   && Value == another.Value
                   && Type == another.Type;
        }
    }
} 