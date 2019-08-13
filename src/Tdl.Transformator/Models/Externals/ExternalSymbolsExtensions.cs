using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Parameters;
using Tdl.Transformator.Services;
using Tdl;

namespace Tdl.Transformator.Models.Externals
{
    public static class ExternalSymbolsExtensions
    {
        [NotNull, ItemNotNull]
        public static IEnumerable<FieldModel> 
            GetExternalFields([NotNull] this ExternalSymbol externalSymbol, [NotNull] ModelContainer modelContainer)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            // we selected field symbols in where
            return
                externalSymbol.MemberTable.AllSymbols
                    .OfType<FieldSymbol>()
                    .Select(symbol => new FieldModel(symbol));
        }
    }
}
