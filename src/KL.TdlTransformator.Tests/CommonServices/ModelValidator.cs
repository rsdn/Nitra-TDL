using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using KL.TdlTransformator.Models;

namespace KL.TdlTransformator.Tests.CommonServices
{
    public sealed class ModelValidator
    {
        private readonly ModelConverter _converter = new ModelConverter();
        private Func<string, string, bool> _comparator;

        public ModelValidator()
        {
            SetDefaultTdlComparator();
        }

        public bool TdlToModelValidate<TModel>(
            [NotNull] string directory,
            [NotNull] string tdlFile,
            [CanBeNull] TModel expectedModel) where TModel : Model
        {
            var model = _converter.ConvertToModelFromTdl<TModel>(directory, new[] { tdlFile });

            return ModelComparator.Compare(expectedModel, model);
        }

        public bool ModelToTdlValidate([CanBeNull] Model model, [CanBeNull] string expectedTdl)
        {
            var generated = model?.Print();

            return _comparator(expectedTdl, generated);
        }

        public bool TdlToTdlValidate([NotNull] string directory, [NotNull] string tdlFile)
        {
            var path = Path.Combine(directory, tdlFile);
            var originalTdl = File.ReadAllText(path);

            var container = _converter.BuildContainer(directory, new[] { tdlFile });
            var module = container.GetPrintable().FirstOrDefault();
            if (module == null)
            {
                throw new InvalidOperationException("no printable model in container");
            }

            var generatedTdl = module.Print();

            return _comparator(originalTdl, generatedTdl);
        }

        public void SetTdlComparator([NotNull] Func<string, string, bool> comparator)
            => _comparator = comparator;

        public void SetDefaultTdlComparator() => _comparator = Compare;

        private bool Compare([NotNull] string original, [NotNull] string generated)
        {
            const string breakLinesPattern = @"\n|\r";
            original = Regex.Replace(original, breakLinesPattern, string.Empty);
            generated = Regex.Replace(generated, breakLinesPattern, string.Empty);

            const string tabPattern = @"\t|\s+";
            const string whitespace = " ";
            original = Regex.Replace(original, tabPattern, whitespace).Trim();
            generated = Regex.Replace(generated, tabPattern, whitespace).Trim();

            return original == generated;
        }
    }
} 
