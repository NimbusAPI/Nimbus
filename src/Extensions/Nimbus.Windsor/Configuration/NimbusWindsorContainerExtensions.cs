using System;
using System.Collections.Generic;
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

            container.Register(
                Classes.From(typeProvider.AllHandlerTypes())
                       .Where(t => true)
                       .WithServiceSelect((t, bt) => SelectHandlerInterfaces(t, typeProvider))
                       .Configure(ConfigureComponent)
                       .LifestyleScoped()
                );

            return container;
        }

        private static IEnumerable<Type> SelectHandlerInterfaces(Type type, ITypeProvider typeProvider)
        {
            var handlerInterfaces = type
                .GetInterfaces()
                .Where(typeProvider.IsClosedGenericHandlerInterface)
                .ToArray();

            return handlerInterfaces;
        }

        private static void ConfigureComponent(ComponentRegistration componentRegistration)
        {
            componentRegistration.Named(componentRegistration.Implementation.FullName);
        }
    }
}