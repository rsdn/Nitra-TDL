using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using KL.TdlTransformator.Models.Parameters;
using KL.TdlTransformator.Services;
using Tdl;

namespace KL.TdlTransformator.Models.Externals
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
                    .Where(symbol => symbol is FieldSymbol)
                    .Select(symbol =>
                        new FieldModel(symbol as FieldSymbol));
        }
    }
}
