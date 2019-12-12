using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.Infrastructure;
using Nimbus.Transports.Amqp.MessageSendersAndRecievers;

namespace Nimbus.Transports.Amqp
{
    public class AmqpTransportConfiguration : TransportConfiguration
    {
        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<AmqpMessageSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<AmqpMessageReciever>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<AmqpTransport>(ComponentLifetime.SingleInstance, typeof (INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}