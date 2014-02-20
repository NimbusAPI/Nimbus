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

            builder.RegisterType<AutofacMulticastEventBroker>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<AutofacCompetingEventBroker>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<AutofacCommandHandlerFactory>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<AutofacRequestBroker>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<AutofacMulticastRequestBroker>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            return builder;
        }
    }
}