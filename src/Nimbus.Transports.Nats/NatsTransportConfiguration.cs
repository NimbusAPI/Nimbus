using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
using Nimbus.DevelopmentStubs;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.Configuration;
using Nimbus.Transports.Nats.ConnectionManagement;
using Nimbus.Transports.Nats.DeadLetterOffice;
using Nimbus.Transports.Nats.DelayedDelivery;
using Nimbus.Transports.Nats.MessageSendersAndReceivers;
using Nimbus.Transports.Nats.QueueManagement;

namespace Nimbus.Transports.Nats
{
    public class NatsTransportConfiguration : TransportConfiguration
    {
        internal NatsUrl NatsUrl { get; set; } = new NatsUrl { Value = "nats://localhost:4222" };

        public NatsTransportConfiguration WithUrl(string url)
        {
            NatsUrl = new NatsUrl { Value = url };
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            container.RegisterType<NatsConnectionFactory>(ComponentLifetime.SingleInstance);

            container.RegisterType<NatsQueueSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<NatsQueueReceiver>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<NatsTopicSender>(ComponentLifetime.InstancePerDependency);
            container.RegisterType<NatsTopicReceiver>(ComponentLifetime.InstancePerDependency);

            container.RegisterType<NatsDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));
            container.RegisterType<NatsDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof(IDeadLetterOffice), typeof(NatsDeadLetterOffice));
            container.RegisterType<NatsNamespaceCleanser>(ComponentLifetime.SingleInstance, typeof(INamespaceCleanser));
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof(ILargeMessageBodyStore));

            container.RegisterType<NatsTransport>(ComponentLifetime.SingleInstance, typeof(INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}
