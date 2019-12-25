using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Tdl.Transformator.Models.Expressions
{
    public sealed class List : Expression
    {
        public List([NotNull] IEnumerable<Expr> list)
        {
            ListValue = list;
        }

        public IEnumerable<Expr> ListValue { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("[");
            foreach (var value in ListValue)
            {
                builder.Append($"{value}");
                if (value != ListValue.Last())
                {
                    builder.Append(", ");
                }
            }
            builder.Append("];");

            return builder.ToString();
        }

        public override bool Equals(Expression other)
        {
            return other is List second && ListValue.SequenceEqual(second.ListValue); 
        }
    }
}
