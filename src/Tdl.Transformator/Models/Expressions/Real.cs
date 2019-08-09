using System;
using System.Globalization;

namespace KL.TdlTransformator.Models.Expressions
{
    public sealed class Real : Expression
    {
        public Real(double value)
        {
            Value = value;
        }

        public double Value { get; }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override bool Equals(Expression other)
        {
            const double minNormal = 0.0000001;
            return other is Real second
                   && (Value.Equals(second.Value)
                    || Math.Abs(Value - second.Value) < minNormal);
        }
    }
}