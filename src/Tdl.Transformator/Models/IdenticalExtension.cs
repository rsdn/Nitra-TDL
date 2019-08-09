using JetBrains.Annotations;
using System.Collections.Generic;

namespace KL.TdlTransformator.Models
{
    public static class IdenticalExtension
    {
        public static bool IsIdentical<TIdentic>(
            [NotNull, ItemNotNull] this IEnumerable<IIdentical<TIdentic>> identicals,
            [NotNull] IEnumerable<TIdentic> identicalsValues)
        {
            if (ReferenceEquals(identicals, identicalsValues))
            {
                return true;
            }

            if (identicals is ICollection<TIdentic> ident
                && identicalsValues is ICollection<TIdentic> otherIdent
                && ident.Count != otherIdent.Count)
            {
                return false;
            }

            using (var enumerator = identicals.GetEnumerator())
            {
                using (var otherEnumerator = identicalsValues.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!(otherEnumerator.MoveNext() && enumerator.Current.IsIdentical(otherEnumerator.Current)))
                        {
                            return false;
                        } 
                    }

                    if (otherEnumerator.MoveNext())
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}