using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.Nats.ConnectionManagement;
using Nimbus.Transports.Nats.MessageSendersAndReceivers;
using Nimbus.Transports.Nats.QueueManagement;

namespace Nimbus.Transports.Nats
{
    internal class NatsTransport : INimbusTransport, IDisposable
    {
        private readonly PoorMansIoC _container;
        private readonly NatsConnectionFactory _connectionFactory;
        private readonly bool _isJetStream;
        private NatsJetStreamRetryStreamReaper? _retryStreamReaper;

        public NatsTransport(PoorMansIoC container, NatsConnectionFactory connectionFactory, NatsTransportConfiguration config)
        {
            _container = container;
            _connectionFactory = connectionFactory;
            _isJetStream = config.IsJetStream;
        }

        public async Task TestConnection()
        {
            await _connectionFactory.TestConnection();

            if (_isJetStream)
            {
                _retryStreamReaper = _container.Resolve<NatsJetStreamRetryStreamReaper>();
                _retryStreamReaper.Start();
            }
        }

        public void Dispose()
        {
            _retryStreamReaper?.Dispose();
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
