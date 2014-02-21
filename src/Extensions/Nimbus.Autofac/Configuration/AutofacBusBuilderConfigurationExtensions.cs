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
                .WithMulticastEventBroker(componentContext.Resolve<IMulticastEventHandlerFactory>())
                .WithCompetingEventHandlerFactory(componentContext.Resolve<ICompetingEventHandlerFactory>())
                .WithCommandHandlerFactory(componentContext.Resolve<ICommandHandlerFactory>())
                .WithRequestBroker(componentContext.Resolve<IRequestBroker>())
                .WithMulticastRequestBroker(componentContext.Resolve<IMulticastRequestHandlerFactory>())
                .WithLogger(componentContext.Resolve<ILogger>());
        }
    }
}