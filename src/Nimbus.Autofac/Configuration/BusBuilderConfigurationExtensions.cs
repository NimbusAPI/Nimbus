using Autofac;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Autofac.Configuration
{
    public static class BusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithAutofacDefaults(this BusBuilderConfiguration configuration, IComponentContext componentContext)
        {
            return configuration
                .WithMulticastEventBroker(componentContext.Resolve<IMulticastEventBroker>())
                .WithCompetingEventBroker(componentContext.Resolve<ICompetingEventBroker>())
                .WithCommandBroker(componentContext.Resolve<ICommandBroker>())
                .WithRequestBroker(componentContext.Resolve<IRequestBroker>())
                .WithMulticastRequestBroker(componentContext.Resolve<IMulticastRequestBroker>())
                .WithLogger(componentContext.Resolve<ILogger>());
        }
    }
}