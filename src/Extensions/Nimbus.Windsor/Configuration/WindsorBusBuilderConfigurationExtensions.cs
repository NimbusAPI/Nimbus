using Castle.Windsor;
using Nimbus.InfrastructureContracts;

// ReSharper disable CheckNamespace
namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class WindsorBusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithWindsorDefaults(this BusBuilderConfiguration configuration, IWindsorContainer container)
        {
            return configuration
                .WithMulticastEventBroker(container.Resolve<IMulticastEventBroker>())
                .WithCompetingEventBroker(container.Resolve<ICompetingEventBroker>())
                .WithCommandBroker(container.Resolve<ICommandBroker>())
                .WithRequestBroker(container.Resolve<IRequestBroker>())
                .WithMulticastRequestBroker(container.Resolve<IMulticastRequestBroker>())
                .WithLogger(container.Resolve<ILogger>());
        }
    }
}