using JetBrains.Annotations;
using KL.TdlTransformator.Models.Suite;
using KL.TdlTransformator.Models.TypedReference;
using Nitra.Declarations;

namespace KL.TdlTransformator.Models.Platforms
{
    public abstract class PlatformBase : SuiteRelatable
    {
        protected PlatformBase([NotNull]DeclarationSymbol symbol, ModelType modelType) 
            : base(symbol, modelType)
        {
        }

        [NotNull]
        public BackReference<PlatformsSetModel> PlatfromSetRefs { get; private set; }

        public override bool IsAnyBackReferenceSet => base.IsAnyBackReferenceSet || PlatfromSetRefs.Any();

        [NotNull]
        protected virtual BackReference<PlatformsSetModel> CreatePlatformRefs() => new BackReference<PlatformsSetModel>();

        protected override void CreateBackReferenceObjects()
        {
            base.CreateBackReferenceObjects();

            PlatfromSetRefs = CreatePlatformRefs();
        }
    }
} 