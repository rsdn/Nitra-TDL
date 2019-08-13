using JetBrains.Annotations;

namespace Tdl.Transformator.Models.Expressions
{
    public sealed class String : Expression
    {
        public String([NotNull]string value)
        {
            Value = value;
        }

        [NotNull]
        public string Value { get; set; }

        public override string ToString()
        {
            return Value.Print();
        }

        public override bool Equals(Expression other)
        {
            return other is String second && Value == second.Value;
        }
    }
}