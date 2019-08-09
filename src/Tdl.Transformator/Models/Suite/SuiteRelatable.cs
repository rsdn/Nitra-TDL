using JetBrains.Annotations;
using KL.TdlTransformator.Models.TypedReference;
using Nitra.Declarations; 

namespace KL.TdlTransformator.Models.Suite
{
    public abstract class SuiteRelatable : Model
    {
        protected SuiteRelatable([NotNull] DeclarationSymbol symbol, ModelType modelType)
          : base(
              modelType,
              symbol.Id,
              symbol.FirstDeclarationOrDefault.Location,
              symbol.Name,
              symbol.FirstDeclarationOrDefault.Location.Source.File.FullName)
        {
        }

        protected SuiteRelatable(
            ModelType modelType,
            int id,
            Nitra.Location? location,
            [NotNull] string name,
            [NotNull] string path)
        : base(modelType, id, location, name, path)
        {
        }

        [NotNull]
        public BackReference<SuiteDefModel> SuiteRefs { get; private set; }

        public override bool HasBackReference => true;

        public override bool IsAnyBackReferenceSet => SuiteRefs.Any();

        [NotNull]
        protected virtual BackReference<SuiteDefModel> CreateSuiteRefs() => new BackReference<SuiteDefModel>();

        protected override void CreateBackReferenceObjects()
        {
            base.CreateBackReferenceObjects();

            SuiteRefs = CreateSuiteRefs();
        }
    }
} 