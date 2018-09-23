using Nimbus.ConcurrentCollections;

namespace Nimbus.Transports.InProcess.QueueManagement
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

        public AsyncBlockingCollection<NimbusMessage> GetOrCreateMessageQueue(string path)
        {
            return _messageQueues.GetOrAdd(path, p => new AsyncBlockingCollection<NimbusMessage>());
        }

        /// <summary>
        ///     Returns the specified queue if it already exists but does not create one if it does not.
        /// </summary>
        /// <returns>True if the queue exists; false otherwise.</returns>
        /// <remarks>
        ///     Senders should use this so that we don't fill up in-memory queues with messages for which there are no handlers.
        ///     Receivers should use GetOrCreateMessageQueue.
        /// </remarks>
        public bool TryGetExistingMessageQueue(string path, out AsyncBlockingCollection<NimbusMessage> queue)
        {
            return _messageQueues.TryGetValue(path, out queue);
        }

        internal void Clear()
        {
            _queues.Clear();
            _topics.Clear();
            _messageQueues.Clear();
        }
    }
}