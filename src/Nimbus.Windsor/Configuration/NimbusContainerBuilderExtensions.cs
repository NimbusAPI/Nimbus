using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Nimbus.Windsor.Infrastructure;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Castle.Windsor;

namespace Nimbus.Windsor.Configuration
{
    public static class NimbusContainerBuilderExtensions
    {
        public static IWindsorContainer RegisterNimbus(this IWindsorContainer container, ITypeProvider typeProvider)
        {
            container.Register(
                Classes.From(typeProvider.AllHandlerTypes()).Where(t => true).WithServiceAllInterfaces().LifestyleScoped(),
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