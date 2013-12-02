using Autofac;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Autofac
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

            builder.RegisterType<AutofacCommandBroker>()
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