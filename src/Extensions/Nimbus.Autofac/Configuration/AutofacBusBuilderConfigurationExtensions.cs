using Autofac;
using Nimbus.HandlerFactories;

// ReSharper disable CheckNamespace

namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class AutofacBusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithAutofacDefaults(this BusBuilderConfiguration configuration, IComponentContext componentContext)
        {
            return configuration
                .WithMulticastEventHandlerFactory(componentContext.Resolve<IMulticastEventHandlerFactory>())
                .WithCompetingEventHandlerFactory(componentContext.Resolve<ICompetingEventHandlerFactory>())
                .WithCommandHandlerFactory(componentContext.Resolve<ICommandHandlerFactory>())
                .WithRequestHandlerFactory(componentContext.Resolve<IRequestHandlerFactory>())
                .WithMulticastRequestHandlerFactory(componentContext.Resolve<IMulticastRequestHandlerFactory>())
                .WithLogger(componentContext.Resolve<ILogger>());
        }
    }
}