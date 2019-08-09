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
