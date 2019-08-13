using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using KL.TdlTransformator.Models;
using KL.TdlTransformator.Services;

namespace KL.TdlTransformator.Tests.CommonServices
{
    public sealed class ModelConverter
    {
        private readonly SymbolConverter _symbolConverter;
        
        public ModelConverter()
        {
            _symbolConverter = new SymbolConverter();
        }

        [NotNull]
        public IEnumerable<TModel> ConvertToModelsFromTdl<TModel>(
            [NotNull] string directory, 
            [NotNull, ItemNotNull] IEnumerable<string> files) where TModel : Model
        {
            var modelContainer = BuildContainer(directory, files);

            return modelContainer.GetAll<TModel>();
        }

        [NotNull]
        public ModelContainer BuildContainer([NotNull] string directory, [NotNull, ItemNotNull] IEnumerable<string> files) 
        {
            var context = Context.GetContext(directory, files);

            var modelContainer = _symbolConverter.ParseSymbols(context);

            return modelContainer;
        }

        [CanBeNull]
        public TModel ConvertToModelFromTdl<TModel>(
            [NotNull] string directory,
            [NotNull, ItemNotNull] IEnumerable<string> files) where TModel : Model 
            => ConvertToModelsFromTdl<TModel>(directory, files).FirstOrDefault();
    }
} 
