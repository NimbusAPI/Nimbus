using System.Linq;
using Autofac;
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
                var handlerInterfaceTypes = handlerType.GetInterfaces().Where(typeProvider.IsHandlerType);
                foreach (var interfaceType in handlerInterfaceTypes)
                {
                    builder.RegisterType(handlerType)
                           .Named(handlerType.FullName, interfaceType)
                           .InstancePerLifetimeScope();
                }
            }

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