using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Windsor.Infrastructure;

// ReSharper disable CheckNamespace

namespace Nimbus.Windsor.Configuration
// ReSharper restore CheckNamespace
{
    public static class NimbusWindsorContainerExtensions
    {
        public static IWindsorContainer RegisterNimbus(this IWindsorContainer container, ITypeProvider typeProvider)
        {
            container.Register(
                Component.For<IDependencyResolver>().ImplementedBy<WindsorDependencyResolver>().LifestyleSingleton(),
                Component.For<ITypeProvider>().Instance(typeProvider).LifestyleSingleton()
                );

            foreach (var handlerType in typeProvider.AllHandlerTypes())
            {
                var handlerInterfaceTypes = handlerType.GetInterfaces().Where(typeProvider.IsClosedGenericHandlerInterface);
                foreach (var interfaceType in handlerInterfaceTypes)
                {
                    container.Register(Component.For(interfaceType).ImplementedBy(handlerType).Named(handlerType.FullName).LifestyleScoped());
                }
            }

            return container;
        }
    }
}