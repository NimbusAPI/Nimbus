using System;
using System.Threading.Tasks;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.Redis.ConnectionManagement;
using Nimbus.Transports.Redis.MessageSendersAndReceivers;
using Nimbus.Transports.Redis.QueueManagement;

namespace Nimbus.Transports.Redis
{
    internal class RedisTransport : INimbusTransport, IDisposable
    {
        private readonly PoorMansIoC _container;
        private readonly ConnectionMultiplexerFactory _connectionMultiplexerFactory;
        private readonly RedisIdleSubscriptionReaper _idleSubscriptionReaper;

        public RedisTransport(PoorMansIoC container, ConnectionMultiplexerFactory connectionMultiplexerFactory,
            RedisIdleSubscriptionReaper idleSubscriptionReaper)
        {
            _container = container;
            _connectionMultiplexerFactory = connectionMultiplexerFactory;
            _idleSubscriptionReaper = idleSubscriptionReaper;
        }

        public async Task TestConnection()
        {
            await _connectionMultiplexerFactory.TestConnection();
            _idleSubscriptionReaper.Start();
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

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName,
            IFilterCondition filter)
        {
            var subscription = new Subscription(topicPath, subscriptionName);
            return _container.ResolveWithOverrides<RedisSubscriptionReceiver>(subscription);
        }

        public void Dispose()
        {
            _idleSubscriptionReaper.Dispose();
        }
    }
}