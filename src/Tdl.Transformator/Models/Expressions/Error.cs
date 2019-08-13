namespace Tdl.Transformator.Models.Expressions
{
    public sealed class Error : Expression
    {
        public override string ToString()
        {
            return "Error";
        }

        public override bool Equals(Expression other)
        {
            return other is Error;
        }
    }
}