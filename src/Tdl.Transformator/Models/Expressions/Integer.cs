namespace KL.TdlTransformator.Models.Expressions
{
    public sealed class Integer : Expression
    {
        public Integer(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(Expression other)
        {
            return other is Integer second && Value == second.Value;
        }
    }
}