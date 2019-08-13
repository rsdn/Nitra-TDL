namespace Tdl.Transformator.Models.Expressions
{
    public sealed class False : Expression
    {
        public override string ToString()
        {
            return "false";
        }

        public override bool Equals(Expression other)
        {
            return other is False;
        }
    }
}