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
            typeProvider.AllHandlerTypes()
                        .Do(t => builder.RegisterType(t)
                                        .AsSelf()
                                        .InstancePerLifetimeScope())
                        .Done();

            typeProvider.InterceptorTypes
                        .Do(t => builder.RegisterType(t)
                                        .AsSelf()
                                        .InstancePerLifetimeScope())
                        .Done();

            builder.RegisterInstance(typeProvider)
                   .As<ITypeProvider>()
                   .SingleInstance();

            builder.RegisterType<AutofacDependencyResolver>()
                   .As<IDependencyResolver>()
                   .SingleInstance();

            return builder;
        }
    }
}