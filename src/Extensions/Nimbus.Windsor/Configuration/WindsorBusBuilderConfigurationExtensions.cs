using Castle.Windsor;
using Nimbus.DependencyResolution;

// ReSharper disable CheckNamespace

namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class WindsorBusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithWindsorDefaults(this BusBuilderConfiguration configuration, IWindsorContainer container)
        {
            return configuration
                .WithDependencyResolver(container.Resolve<IDependencyResolver>())
                .WithLogger(container.Resolve<ILogger>());
        }
    }
}