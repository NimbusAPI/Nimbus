using Microsoft.Practices.Unity;
using Nimbus.DependencyResolution;
using Nimbus.Unity.Infastructure;

namespace Nimbus.Unity.Configuration
{
    public static class UnityContainerExtensions
    {
        public static T RegisterNimbus<T>(this T container, ITypeProvider typeProvider) where T : IUnityContainer
        {
            var dependencyResolver = new UnityDependencyResolver(container);
            container.RegisterInstance<IDependencyResolver>(dependencyResolver);
            container.RegisterInstance<ITypeProvider>(typeProvider);

            return container;
        }
    }
}