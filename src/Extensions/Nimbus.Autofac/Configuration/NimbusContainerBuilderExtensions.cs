using Autofac;
using Nimbus.Autofac.Infrastructure;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

// ReSharper disable CheckNamespace
namespace Nimbus.Configuration
// ReSharper restore CheckNamespace
{
    public static class NimbusContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterNimbus(this ContainerBuilder builder, ITypeProvider typeProvider)
        {
            builder.RegisterTypes(typeProvider.AllHandlerTypes())
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();

            builder.RegisterType<AutofacMulticastEventHandlerFactory>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<AutofacCompetingEventHandlerFactory>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<AutofacCommandHandlerFactory>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<AutofacRequestHandlerFactory>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<AutofacMulticastRequestHandlerFactory>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            return builder;
        }
    }
}