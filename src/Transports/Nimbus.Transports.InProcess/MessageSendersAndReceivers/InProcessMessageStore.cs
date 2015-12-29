using Nimbus.ConcurrentCollections;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessMessageStore
    {
        private readonly ThreadSafeDictionary<string, Queue> _queues = new ThreadSafeDictionary<string, Queue>();
        private readonly ThreadSafeDictionary<string, Topic> _topics = new ThreadSafeDictionary<string, Topic>();

        private readonly ThreadSafeDictionary<string, AsyncBlockingCollection<NimbusMessage>> _messageQueues =
            new ThreadSafeDictionary<string, AsyncBlockingCollection<NimbusMessage>>();

        public Queue GetQueue(string queuePath)
        {
            return _queues.GetOrAdd(queuePath, p => new Queue(queuePath));
        }

        public Topic GetTopic(string topicPath)
        {
            return _topics.GetOrAdd(topicPath, p => new Topic(topicPath));
        }

        public AsyncBlockingCollection<NimbusMessage> GetMessageQueue(string path)
        {
            return _messageQueues.GetOrAdd(path, p => new AsyncBlockingCollection<NimbusMessage>());
        }
    }
}