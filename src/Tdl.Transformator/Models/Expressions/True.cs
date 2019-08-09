namespace KL.TdlTransformator.Models.Expressions
{
    public sealed class True : Expression
    {
        public override string ToString()
        {
            return "true";
        }

        public override bool Equals(Expression other)
        {
            return other is True;
        }
    }
}