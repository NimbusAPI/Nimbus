using System.Threading.Tasks;
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

        public NatsTransport(PoorMansIoC container, NatsConnectionFactory connectionFactory)
        {
            _container = container;
            _connectionFactory = connectionFactory;
        }

        public Task TestConnection()
        {
            return _connectionFactory.TestConnection();
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return _container.ResolveWithOverrides<NatsQueueSender>(queuePath);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return _container.ResolveWithOverrides<NatsQueueReceiver>(queuePath);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            return _container.ResolveWithOverrides<NatsTopicSender>(topicPath);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            var subscription = new NatsSubscription(topicPath, subscriptionName);
            return _container.ResolveWithOverrides<NatsTopicReceiver>(subscription);
        }
    }
}
