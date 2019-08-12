using Nitra;
using Nitra.Declarations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KL.TdlTransformator
{
    static class Utils
    {
        public static bool All2<T>(this IEnumerable<T> xs, IEnumerable<T> ys, Func<T, T, bool> equals)
        {
            var iteraror1 =  xs.GetEnumerator();
            var iteraror2 =  ys.GetEnumerator();
            var state1 = iteraror1.MoveNext();
            var state2 = iteraror2.MoveNext();

            while (state1 && state2)
            {
                if (!equals(iteraror1.Current, iteraror2.Current))
                {
                    return false;
                }
            }

            return state1 = state2;
        }

        public static SourceSnapshot GetSource(this DeclarationSymbol symbol)
        {
            return symbol.FirstDeclarationOrDefault.Location.Source;
        }

        public static SourceSnapshot GetSource(this IAst symbol)
        {
            return symbol.Location.Source;
        }
    }
}
