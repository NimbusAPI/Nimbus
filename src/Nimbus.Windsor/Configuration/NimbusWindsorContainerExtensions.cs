using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Nimbus.Windsor.Infrastructure;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
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
                Component.For<IMulticastEventBroker>().ImplementedBy<WindsorMulticastEventBroker>().LifestyleSingleton(),
                Component.For<ICompetingEventBroker>().ImplementedBy<WindsorCompetingEventBroker>().LifestyleSingleton(),
                Component.For<ICommandBroker>().ImplementedBy<WindsorCommandBroker>().LifestyleSingleton(),
                Component.For<IRequestBroker>().ImplementedBy<WindsorRequestBroker>().LifestyleSingleton(),
                Component.For<IMulticastRequestBroker>().ImplementedBy<WindsorMulticastRequestBroker>().LifestyleSingleton()
                );

            return container;
        }
    }
}