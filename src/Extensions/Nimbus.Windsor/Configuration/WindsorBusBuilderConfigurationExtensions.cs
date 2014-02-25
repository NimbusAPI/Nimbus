using Castle.Windsor;
using Nimbus.HandlerFactories;

// ReSharper disable CheckNamespace

namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class WindsorBusBuilderConfigurationExtensions
    {
        public static BusBuilderConfiguration WithWindsorDefaults(this BusBuilderConfiguration configuration, IWindsorContainer container)
        {
            return configuration
                .WithMulticastEventHandlerFactory(container.Resolve<IMulticastEventHandlerFactory>())
                .WithCompetingEventHandlerFactory(container.Resolve<ICompetingEventHandlerFactory>())
                .WithCommandHandlerFactory(container.Resolve<ICommandHandlerFactory>())
                .WithRequestHandlerFactory(container.Resolve<IRequestHandlerFactory>())
                .WithMulticastRequestHandlerFactory(container.Resolve<IMulticastRequestHandlerFactory>())
                .WithLogger(container.Resolve<ILogger>());
        }
    }
}