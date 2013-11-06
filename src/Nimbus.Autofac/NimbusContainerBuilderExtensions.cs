using Autofac;
using Nimbus.Configuration;
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
                   .As<IMulticastEventBroker>()
                   .SingleInstance();

            builder.RegisterType<AutofacCommandBroker>()
                   .As<ICommandBroker>()
                   .SingleInstance();

            builder.RegisterType<AutofacRequestBroker>()
                   .As<IRequestBroker>()
                   .SingleInstance(); 
            
            return builder;
        }
    }
}