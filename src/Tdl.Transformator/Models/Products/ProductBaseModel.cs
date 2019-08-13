using JetBrains.Annotations;
using Tdl.Transformator.Models.Suite;
using Tdl.Transformator.Models.TypedReference;
using Nitra.Declarations;

namespace Tdl.Transformator.Models.Products
{
    public abstract class ProductBaseModel : SuiteRelatable
    {
        protected ProductBaseModel([NotNull]DeclarationSymbol symbol, ModelType modelType) 
            : base(symbol, modelType)
        {
        }

        [NotNull]
        public BackReference<ProductSetModel> ProductSetRefs { get; private set; }

        public override bool IsAnyBackReferenceSet => base.IsAnyBackReferenceSet || ProductSetRefs.Any();

        [NotNull]
        protected virtual BackReference<ProductSetModel> CreateProductSetRefs() => new BackReference<ProductSetModel>();

        protected override void CreateBackReferenceObjects()
        {
            base.CreateBackReferenceObjects();

            ProductSetRefs = CreateProductSetRefs();
        }
    }
} 