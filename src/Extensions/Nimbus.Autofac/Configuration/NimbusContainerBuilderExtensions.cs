using System.Linq;
using Autofac;
using Autofac.Features.Variance;
using Nimbus.Autofac.Infrastructure;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;

// ReSharper disable CheckNamespace

namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class NimbusContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterNimbus(this ContainerBuilder builder, ITypeProvider typeProvider)
        {
            foreach (var handlerType in typeProvider.AllHandlerTypes())
            {
                var handlerInterfaceTypes = handlerType.GetInterfaces().Where(typeProvider.IsClosedGenericHandlerInterface);
                foreach (var interfaceType in handlerInterfaceTypes)
                {
                    builder.RegisterType(handlerType)
                           .Named(handlerType.FullName, interfaceType)
                           .InstancePerLifetimeScope();
                }
            }

            builder.RegisterSource(new ContravariantRegistrationSource());
            typeProvider.InterceptorTypes
                        .Do(t => builder.RegisterType(t)
                                        .AsSelf()
                                        .InstancePerLifetimeScope())
                        .Done();

            builder.RegisterInstance(typeProvider)
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<AutofacDependencyResolver>()
                   .As<IDependencyResolver>()
                   .SingleInstance();

            return builder;
        }
    }
}