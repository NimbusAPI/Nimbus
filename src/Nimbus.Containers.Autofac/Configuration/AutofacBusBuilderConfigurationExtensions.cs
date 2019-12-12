using Autofac;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.DependencyResolution;

// ReSharper disable CheckNamespace

namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class AutofacBusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithAutofacDefaults(this BusBuilderConfiguration configuration, IComponentContext componentContext)
        {
            return configuration
                .WithTypesFrom(componentContext.Resolve<ITypeProvider>())
                .WithDependencyResolver(componentContext.Resolve<IDependencyResolver>())
                ;
        }
    }
}