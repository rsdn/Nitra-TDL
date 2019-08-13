using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Tdl.Transformator.Models.Parameters;
using Tdl.Transformator.Models.Platforms;
using Tdl.Transformator.Models.Products;
using Tdl.Transformator.Models.Scenario;
using Tdl.Transformator.Services;
using Nitra;
using Nitra.Declarations;
using Tdl;

namespace Tdl.Transformator.Models.Suite
{
    public abstract class SuiteModel : Model
    {
        protected SuiteModel([NotNull] DeclarationSymbol symbol, ModelType modelType) : base(symbol, modelType)
        {
        }

        protected SuiteModel(ModelType modelType, int id, Location? location, [NotNull] string name, [NotNull] string path) : base(modelType, id, location, name, path)
        {
        }
    }
}
