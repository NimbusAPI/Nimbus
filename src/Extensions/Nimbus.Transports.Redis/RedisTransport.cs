using System.Threading.Tasks;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.Redis.MessageSendersAndReceivers;

namespace Nimbus.Transports.Redis
{
    internal class RedisTransport : INimbusTransport
    {
        private readonly PoorMansIoC _container;

        public RedisTransport(PoorMansIoC container)
        {
            _container = container;
        }

        public Task TestConnection()
        {
            return Task.Delay(0);
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

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName)
        {
            var subscription = new Subscription(topicPath, subscriptionName);
            return _container.ResolveWithOverrides<RedisSubscriptionReceiver>(subscription);
        }
    }
}