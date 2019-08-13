using System;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Expressions;
using Nitra;

namespace Tdl.Transformator.Models.Parameters
{
    public sealed class DefinitionModel : IEquatable<DefinitionModel>, ICloneable
    {
        public DefinitionModel([NotNull] string name, [CanBeNull] Expression expression, Location? location)
        {
            Name = name;
            Expression = expression;
            Comments = new CommentBlockModel(location);
        }

        [NotNull]
        public string Name { get; }

        [CanBeNull]
        public Expression Expression { get; set; }

        [NotNull]
        public CommentBlockModel Comments { get; set; }

        public bool Equals(DefinitionModel other)
        {
            if (Name != other?.Name)
            {
                return false;
            }

            if (Expression != null && other.Expression != null)
            {
                return Expression.Equals(other.Expression);
            }

            return false;
        }

        public override string ToString()
        {
            return $"{Name} = {Expression}";
        }

        [NotNull]
        public string Print()
        {
            var builder = new StringBuilder();
            Comments.AppendComments(builder);
            builder.Append($"{Name} = {Expression}");
            return builder.ToString();
        }

        public object Clone()
        {
            var expression = (Expression)Expression.Clone();
            var comments = (CommentBlockModel)Comments.Clone();

            var clone = new DefinitionModel(Name, expression, null) { Comments = comments };

            return clone;
        }
    }
} 