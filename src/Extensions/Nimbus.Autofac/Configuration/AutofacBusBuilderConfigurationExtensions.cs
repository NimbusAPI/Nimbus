using Autofac;
using Nimbus.InfrastructureContracts;

// ReSharper disable CheckNamespace
namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class AutofacBusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithAutofacDefaults(this BusBuilderConfiguration configuration, IComponentContext componentContext)
        {
            return configuration
                .WithMulticastEventBroker(componentContext.Resolve<IMulticastEventBroker>())
                .WithCompetingEventBroker(componentContext.Resolve<ICompetingEventBroker>())
                .WithCommandHandlerFactory(componentContext.Resolve<ICommandHandlerFactory>())
                .WithRequestBroker(componentContext.Resolve<IRequestBroker>())
                .WithMulticastRequestBroker(componentContext.Resolve<IMulticastRequestBroker>())
                .WithLogger(componentContext.Resolve<ILogger>());
        }
    }
}