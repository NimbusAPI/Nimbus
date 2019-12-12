using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Amqp.DeadLetterOffice;
using Nimbus.Transports.Amqp.DelayedDelivery;
using Nimbus.Transports.Amqp.MessageSendersAndRecievers;

namespace Nimbus.Transports.Amqp
{
    public class AmqpTransportConfiguration : TransportConfiguration
    {
        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<AmqpMessageSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<AmqpMessageReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<AmqpTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
            container.RegisterType<AmqpDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof (IDeadLetterOffice));
            container.RegisterType<AmqpDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof (IDelayedDeliveryService));
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof (ILargeMessageBodyStore));
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}