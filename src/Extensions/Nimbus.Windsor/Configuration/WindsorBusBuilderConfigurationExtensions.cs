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
                .WithCompetingEventHandlerFactory(container.Resolve<ICompetingEventHandlerFactory>())
                .WithCommandHandlerFactory(container.Resolve<ICommandHandlerFactory>())
                .WithRequestBroker(container.Resolve<IRequestBroker>())
                .WithMulticastRequestBroker(container.Resolve<IMulticastRequestBroker>())
                .WithLogger(container.Resolve<ILogger>());
        }
    }
}