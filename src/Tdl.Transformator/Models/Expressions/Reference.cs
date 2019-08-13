using JetBrains.Annotations;

namespace Tdl.Transformator.Models.Expressions
{
    public sealed class Reference : Expression
    {
        public Reference([NotNull]string name)
        {
            Name = name;
        }

        [NotNull]
        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(Expression other)
        {
            return other is Reference second && Name == second.Name;
        }
    }
}