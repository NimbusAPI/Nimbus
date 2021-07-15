using System.Collections.Generic;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Amqp.ConnectionManagement;
using Nimbus.Transports.Amqp.DeadLetterOffice;
using Nimbus.Transports.Amqp.DelayedDelivery;
using Nimbus.Transports.Amqp.Messages;
using Nimbus.Transports.Amqp.SendersAndReceivers;

namespace Nimbus.Transports.Amqp
{
    public class AmqpTransportConfiguration : TransportConfiguration
    {
        internal ConnectionStringSetting ConnectionString { get; set; }
        
        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<AmqpMessageSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<AmqpMessageReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<AmqpTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
            container.RegisterType<AmqpDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof (IDeadLetterOffice));
            container.RegisterType<MessageFactory>(ComponentLifetime.SingleInstance, typeof (IMessageFactory));
            container.RegisterType<AmqpDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof (IDelayedDeliveryService));
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof (ILargeMessageBodyStore));
            container.RegisterType<ConnectionManager>(ComponentLifetime.SingleInstance, typeof(IConnectionManager));
        }
        
        public AmqpTransportConfiguration WithConnectionString(string connectionString)
        {
            ConnectionString = new ConnectionStringSetting {Value = connectionString};
            return this;
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}