using Nitra.Declarations;
using Tdl.Transformator.Models.Expressions;
using Tdl;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Tdl.Transformator.Models.Parameters
{
    public static class DefinitionGenerator
    {
        [ItemNotNull, NotNull]
        public static List<DefinitionModel> GetDefinitions(
            ImmutableArray<Def> defs,
            [ItemNotNull, NotNull]
            IAstList<Definition> definitions)
        {
            var defMap = defs.ToDictionary(d => d.Symbol, d => d.Expr);
            var definitionModels = new List<DefinitionModel>();

            foreach (var def in definitions)
            {
                var sym = def.SymbolRef.Symbol;
                var expr = defMap[sym];
                definitionModels.Add(new DefinitionModel(def.Reference.Text, expr.ToExpression(), def.Location));
            }

            return definitionModels;
        }

        [ItemNotNull, NotNull]
        public static List<DefinitionModel> GetDefinitions(
            ImmutableArray<UntypedDef> defs,
            [ItemNotNull, NotNull]
            Definition.IAstList definitions)
        {
            if (defs.Count() != definitions.Count())
            {
                throw new InvalidOperationException("counts aren't equal");
            }

            var definitionModels = new List<DefinitionModel>();
            for (var i = 0; i < defs.Count(); i++)
            {
                var def2 = definitions.ElementAt(i);
                definitionModels.Add(new DefinitionModel(
                    def2.Reference.Text, defs[i].Expr.ToExpression(), def2.Location));
            }

            return definitionModels;
        }
    }
}
