using System.Collections.Concurrent;
using Nimbus.ConcurrentCollections;
using Nimbus.Infrastructure;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessMessageStore
    {
        private readonly ThreadSafeDictionary<string, BlockingCollection<NimbusMessage>> _queues = new ThreadSafeDictionary<string, BlockingCollection<NimbusMessage>>();
        private readonly ThreadSafeDictionary<string, ConcurrentBag<string>> _topics = new ThreadSafeDictionary<string, ConcurrentBag<string>>();

        public BlockingCollection<NimbusMessage> GetQueue(string queuePath)
        {
            return _queues.GetOrAdd(queuePath, p => new BlockingCollection<NimbusMessage>());
        }

        public ConcurrentBag<string> GetTopic(string topicPath)
        {
            return _topics.GetOrAdd(topicPath, p => new ConcurrentBag<string>());
        }

        public BlockingCollection<NimbusMessage> GetSubscriptionQueue(string topicPath, string subscriptionName)
        {
            var queuePath = topicPath + "/" + subscriptionName;
            return GetQueue(queuePath);
        }
    }
}