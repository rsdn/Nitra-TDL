using JetBrains.Annotations;
using Tdl.Transformator.Models.Platforms;
using Tdl.Transformator.Models.Products;
using Tdl.Transformator.Services;

namespace Tdl.Transformator.Models.Scenario
{
    public sealed class ScenarioEnvironmentModel : IdenticalBase<ScenarioEnvironmentModel>
    {
        private readonly int _platformId;
        private readonly int _productId;

        // ReSharper disable once NotNullMemberIsNotInitialized
        public ScenarioEnvironmentModel(Tdl.Environment environment)
        {
            _platformId = environment.Platform.Id;
            _productId = environment.Product.Id;
        }

        public ScenarioEnvironmentModel()
        {
            _platformId = 0;
            _productId = 0;
        }

        [NotNull]
        public PlatformBase Platform { get; set; }

        [NotNull]
        public ProductBaseModel Product { get; set; }

        [NotNull]
        public string Print()
        {
            return $"({Platform.Name}, {Product.Name})";
        }

        public void Init([NotNull] ModelContainer modelContainer)
        {
            Platform = modelContainer.Get<PlatformBase>(_platformId);
            Product = modelContainer.Get<ProductBaseModel>(_productId);
        }

        protected override bool IsIdentic(ScenarioEnvironmentModel other)
        {
            return Product.IsIdentical(other.Product)
                   && Platform.IsIdentical(other.Platform);
        }
    }
} 