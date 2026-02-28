using Autofac;
using Nimbus.Containers.Autofac.Infrastructure;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.DependencyResolution;

// ReSharper disable CheckNamespace

namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class NimbusContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterNimbus(this ContainerBuilder builder, ITypeProvider typeProvider)
        {
            builder.RegisterInstance(typeProvider)
                   .As<ITypeProvider>()
                   .SingleInstance();

            builder.RegisterType<AutofacDependencyResolver>()
                   .As<IDependencyResolver>()
                   .SingleInstance();

            typeProvider.AllResolvableTypes()
                        .Do(t => builder.RegisterType(t)
                                        .AsSelf()
                                        .InstancePerLifetimeScope())
                        .Done();

            return builder;
        }
    }
}