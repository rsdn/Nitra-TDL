using JetBrains.Annotations;
using Tdl;

namespace Tdl.Transformator.Models.Expressions
{
    internal static class ExpressionExtensions
    {
        [CanBeNull]
        public static Expression ToExpression([NotNull] this Expr expr)
        {
            switch (expr)
            {
                case Expr.Error _:
                    return new Error();
                case Expr.True _:
                    return new True();
                case Expr.False _:
                    return new False();
                case Expr.Integer i:
                    return new Integer(i.Value);
                case Expr.Real r:
                    return new Real(r.Value);
                case Expr.Reference reference:
                    return new Reference(reference.Symbol.Name); // FIXME: Не хорошая идея ссылаться по имени. Нужно ссылаться на объект.
                case Expr.String s:
                    return new String(s.Value);
                default: return null;
            }
        }
    }
}
