using Nimbus.ConcurrentCollections;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessMessageStore
    {
        private readonly ThreadSafeDictionary<string, Queue> _queues = new ThreadSafeDictionary<string, Queue>();
        private readonly ThreadSafeDictionary<string, Topic> _topics = new ThreadSafeDictionary<string, Topic>();

        public Queue GetQueue(string queuePath)
        {
            return _queues.GetOrAdd(queuePath, p => new Queue());
        }

        public Topic GetTopic(string topicPath)
        {
            return _topics.GetOrAdd(topicPath, p => new Topic());
        }
    }
}