using System.Threading.Tasks;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Filtering.Conditions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.Redis.ConnectionManagement;
using Nimbus.Transports.Redis.MessageSendersAndReceivers;

namespace Nimbus.Transports.Redis
{
    internal class RedisTransport : INimbusTransport
    {
        private readonly PoorMansIoC _container;
        private readonly ConnectionMultiplexerFactory _connectionMultiplexerFactory;

        public RedisTransport(PoorMansIoC container, ConnectionMultiplexerFactory connectionMultiplexerFactory)
        {
            _container = container;
            _connectionMultiplexerFactory = connectionMultiplexerFactory;
        }

        public Task TestConnection()
        {
            return _connectionMultiplexerFactory.TestConnection();
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return _container.ResolveWithOverrides<RedisMessageSender>(queuePath);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return _container.ResolveWithOverrides<RedisMessageReceiver>(queuePath);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            return _container.ResolveWithOverrides<RedisTopicSender>(topicPath);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            var subscription = new Subscription(topicPath, subscriptionName);
            return _container.ResolveWithOverrides<RedisSubscriptionReceiver>(subscription);
        }
    }
}