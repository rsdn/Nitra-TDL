using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using KL.TdlTransformator.Models.Parameters;
using KL.TdlTransformator.Models.Platforms;
using KL.TdlTransformator.Models.Products;
using KL.TdlTransformator.Models.Scenario;
using KL.TdlTransformator.Services;
using Nitra;
using Nitra.Declarations;
using Tdl;

namespace KL.TdlTransformator.Models.Suite
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
