using System;
using System.Text;
using JetBrains.Annotations;
using Nitra;
using Nitra.Declarations;
using Tdl;

namespace Tdl.Transformator.Models
{
    public sealed class ValueModel : IIdentical<ValueModel>, ICloneable
    {
        public ValueModel([CanBeNull]object value, TypesOfValue type, Location location)
        {
            Value = value;
            Type = type;
            Comments = new CommentBlockModel(location);
        }

        public ValueModel(DeclarationSymbol symbol, ParsedValue<string> value) : this(symbol.GetSource(), value)
        {
        }

        public ValueModel(SourceSnapshot source, ParsedValue<string> value)
        {
            var location = new Location(source, value.Span);
            Value = value.ValueOrDefault;
            Type = TypesOfValue.Path; // FIXME: Дичь какая-то. Зачем это?
            Comments = new CommentBlockModel(location);
        }

        [CanBeNull]
        public object Value { get; }

        public TypesOfValue Type { get; }

        [NotNull]
        public CommentBlockModel Comments { get; set; }

        public bool HasValue => Value != null;

        public bool IsIdentical(ValueModel other)
        {
            return Type == other?.Type
                   && Value.ToString() == other.Value.ToString();
        }

        public override string ToString()
        {
            return HasValue
                ? Print()
                : string.Empty;
        }

        [NotNull]
        public string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            if (HasValue)
            {
                switch (Type)
                {
                    case TypesOfValue.Path:
                        builder.Append(GetValue("script @"));
                        break;
                    case TypesOfValue.Success:
                        builder.Append(GetValue("expected "));
                        break;
                    case TypesOfValue.Reboot:
                        builder.Append(GetValue("expected for reboot "));
                        break;
                    case TypesOfValue.Timeout:
                        builder.Append(GetValue("timeout "));
                        break;
                }
            }

            return builder.ToString();
        }

        public object Clone()
        {
            var clone = (ValueModel)MemberwiseClone();
            clone.Comments = (CommentBlockModel)Comments.Clone();

            return clone;
        }

        [NotNull]
        private string GetValue([NotNull]string prefix)
        {
            return Value is string
                ? $"{Constants.TabulationSymbol}{prefix}\"{Value}\""
                : $"{Constants.TabulationSymbol}{prefix}{Value}";
        }       
    }
}
