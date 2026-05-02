using NATS.Client.Core;
using Nimbus.Configuration;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Transport;
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
        internal NatsAuthOpts NatsAuthOpts { get; private set; } = NatsAuthOpts.Default;
        internal bool IsJetStream { get; private set; }

        public NatsTransportConfiguration WithUrl(string url)
        {
            NatsUrl = new NatsUrl { Value = url };
            return this;
        }

        public NatsTransportConfiguration WithCredentials(string username, string password)
        {
            NatsAuthOpts = new NatsAuthOpts { Username = username, Password = password };
            return this;
        }

        public NatsTransportConfiguration WithToken(string token)
        {
            NatsAuthOpts = new NatsAuthOpts { Token = token };
            return this;
        }

        public NatsTransportConfiguration WithNKey(string seed)
        {
            NatsAuthOpts = new NatsAuthOpts { Seed = seed };
            return this;
        }

        public NatsTransportConfiguration WithCredentialsFile(string path)
        {
            NatsAuthOpts = new NatsAuthOpts { CredsFile = path };
            return this;
        }

        public NatsTransportConfiguration WithJetStream()
        {
            IsJetStream = true;
            return this;
        }

        protected override void RegisterComponents(PoorMansIoC container)
        {
            // Register self so NatsConnectionFactory and NatsTransport can take it as a dependency.
            container.Register(this);

            container.RegisterType<NatsConnectionFactory>(ComponentLifetime.SingleInstance);

            if (IsJetStream)
            {
                container.RegisterType<NatsJetStreamContextFactory>(ComponentLifetime.SingleInstance);
                container.RegisterType<NatsJetStreamQueueSender>(ComponentLifetime.InstancePerDependency);
                container.RegisterType<NatsJetStreamQueueReceiver>(ComponentLifetime.InstancePerDependency);
                container.RegisterType<NatsJetStreamTopicSender>(ComponentLifetime.InstancePerDependency);
                container.RegisterType<NatsJetStreamTopicReceiver>(ComponentLifetime.InstancePerDependency);
                container.RegisterType<NatsJetStreamDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));
                container.RegisterType<NatsJetStreamDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof(IDeadLetterOffice));
                container.RegisterType<NatsJetStreamNamespaceCleanser>(ComponentLifetime.SingleInstance, typeof(INamespaceCleanser));
            }
            else
            {
                container.RegisterType<NatsQueueReceiver>(ComponentLifetime.InstancePerDependency);
                container.RegisterType<NatsMessageSender>(ComponentLifetime.InstancePerDependency);
                container.RegisterType<NatsTopicReceiver>(ComponentLifetime.InstancePerDependency);
                container.RegisterType<NatsDelayedDeliveryService>(ComponentLifetime.SingleInstance, typeof(IDelayedDeliveryService));
                container.RegisterType<NatsDeadLetterOffice>(ComponentLifetime.SingleInstance, typeof(IDeadLetterOffice));
                container.RegisterType<NatsNamespaceCleanser>(ComponentLifetime.SingleInstance, typeof(INamespaceCleanser));
            }
            container.RegisterType<UnsupportedLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof(ILargeMessageBodyStore));

            container.RegisterType<NatsTransport>(ComponentLifetime.SingleInstance, typeof(INimbusTransport));
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}
