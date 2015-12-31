using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.InProcess.QueueManagement;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessQueueSender : INimbusMessageSender
    {
        private readonly ISerializer _serializer;
        private readonly Queue _queue;
        private readonly InProcessMessageStore _messageStore;

        public InProcessQueueSender(ISerializer serializer, Queue queue, InProcessMessageStore messageStore)
        {
            _serializer = serializer;
            _queue = queue;
            _messageStore = messageStore;
        }

        public async Task Send(NimbusMessage message)
        {
            var messageClone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
            AsyncBlockingCollection<NimbusMessage> messageQueue;
            if (!_messageStore.TryGetExistingMessageQueue(_queue.QueuePath, out messageQueue)) return;
            await messageQueue.Add(messageClone);
        }
    }
}