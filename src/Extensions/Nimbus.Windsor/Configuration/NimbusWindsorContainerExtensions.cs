using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Nimbus.HandlerFactories;
using Nimbus.Windsor.Infrastructure;
using Nimbus.Extensions;
using Castle.Windsor;

// ReSharper disable CheckNamespace
namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class NimbusWindsorContainerExtensions
    {
        public static IWindsorContainer RegisterNimbus(this IWindsorContainer container, ITypeProvider typeProvider)
        {
            container.Register(
                Classes.From(typeProvider.AllHandlerTypes()).Pick().WithServiceAllInterfaces().LifestyleScoped(),
                Component.For<IMulticastEventHandlerFactory>().ImplementedBy<WindsorMulticastEventHandlerFactory>().LifestyleSingleton(),
                Component.For<ICompetingEventHandlerFactory>().ImplementedBy<WindsorCompetingEventHandlerFactory>().LifestyleSingleton(),
                Component.For<ICommandHandlerFactory>().ImplementedBy<WindsorCommandHandlerFactory>().LifestyleSingleton(),
                Component.For<IRequestHandlerFactory>().ImplementedBy<WindsorRequestHandlerFactory>().LifestyleSingleton(),
                Component.For<IMulticastRequestHandlerFactory>().ImplementedBy<WindsorMulticastRequestHandlerFactory>().LifestyleSingleton()
                );

            return container;
        }
    }
}