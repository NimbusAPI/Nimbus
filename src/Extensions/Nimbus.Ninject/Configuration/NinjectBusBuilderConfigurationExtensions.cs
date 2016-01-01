using Nimbus.DependencyResolution;
using Ninject;

// ReSharper disable once CheckNamespace

namespace Nimbus.Configuration
{
    public static class NinjectBusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithNinjectDefaults(this BusBuilderConfiguration configuration, IKernel kernel)
        {
            return configuration
                .WithDependencyResolver(kernel.Get<IDependencyResolver>())
                .WithTypesFrom(kernel.Get<ITypeProvider>());
        }
    }
}