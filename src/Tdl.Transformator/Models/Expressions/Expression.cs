using JetBrains.Annotations;
using System;

namespace Tdl.Transformator.Models.Expressions
{
    public abstract class Expression : IEquatable<Expression>, ICloneable
    {
        public abstract bool Equals(Expression other);

        [NotNull]
        public static Expression Create([NotNull] object value)
        {
            switch (value)
            {
                case int val: return new Integer(val);
                case string val: return new String(val);
                case double val: return new Real(val);
                default: throw new InvalidOperationException($"Unsupported type [{value.GetType()}]");
            }
        }

        public virtual object Clone() => MemberwiseClone();
    }
} 