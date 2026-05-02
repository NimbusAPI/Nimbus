using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.Nats.ConnectionManagement;
using Nimbus.Transports.Nats.MessageSendersAndReceivers;

namespace Nimbus.Transports.Nats
{
    internal class NatsTransport : INimbusTransport
    {
        private readonly PoorMansIoC _container;
        private readonly NatsConnectionFactory _connectionFactory;
        private readonly bool _isJetStream;

        public NatsTransport(PoorMansIoC container, NatsConnectionFactory connectionFactory, NatsTransportConfiguration config)
        {
            _container = container;
            _connectionFactory = connectionFactory;
            _isJetStream = config.IsJetStream;
        }

        public Task TestConnection()
        {
            return _connectionFactory.TestConnection();
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return _isJetStream
                ? _container.ResolveWithOverrides<NatsJetStreamQueueSender>(queuePath)
                : _container.ResolveWithOverrides<NatsMessageSender>(queuePath);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return _isJetStream
                ? _container.ResolveWithOverrides<NatsJetStreamQueueReceiver>(queuePath)
                : _container.ResolveWithOverrides<NatsQueueReceiver>(queuePath);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            return _isJetStream
                ? _container.ResolveWithOverrides<NatsJetStreamTopicSender>(topicPath)
                : _container.ResolveWithOverrides<NatsMessageSender>(topicPath);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            var subscription = new NatsSubscription(topicPath, subscriptionName);
            return _isJetStream
                ? _container.ResolveWithOverrides<NatsJetStreamTopicReceiver>(subscription)
                : _container.ResolveWithOverrides<NatsTopicReceiver>(subscription);
        }
    }
}
