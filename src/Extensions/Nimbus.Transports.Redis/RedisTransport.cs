using System;
using System.Threading.Tasks;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.Redis.MessageSendersAndReceivers;
using Nimbus.Transports.Redis.QueueManagement;

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
            var queue = new Queue(queuePath);
            return _container.ResolveWithOverrides<RedisMessageSender>(queue);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            throw new NotImplementedException();
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            throw new NotImplementedException();
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName)
        {
            throw new NotImplementedException();
        }
    }
}