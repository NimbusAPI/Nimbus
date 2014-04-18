using Autofac;
using Nimbus.DependencyResolution;

// ReSharper disable CheckNamespace

namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class AutofacBusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithAutofacDefaults(this BusBuilderConfiguration configuration, IComponentContext componentContext)
        {
            return configuration
                .WithDependencyResolver(componentContext.Resolve<IDependencyResolver>())
                .WithLogger(componentContext.Resolve<ILogger>());
        }
    }
}