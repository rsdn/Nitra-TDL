using Unity;
using Unity.Extension;

namespace Tdl.Transformator.Tests.CommonServices
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
