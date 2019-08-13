using Unity;
using Unity.Extension;

namespace KL.TdlTransformator.Tests.CommonServices
{
    public sealed class ContainerExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<ModelValidator>();
            Container.RegisterType<ModelConverter>();
        }
    }
} 
