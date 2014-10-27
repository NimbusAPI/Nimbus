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

            container.Register(
                Classes.From(typeProvider.AllHandlerTypes())
                       .Where(t => true)
                       .LifestyleScoped()
                );

            return container;
        }
    }
}