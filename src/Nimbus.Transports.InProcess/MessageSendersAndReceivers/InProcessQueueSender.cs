using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.InProcess.QueueManagement;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessQueueSender : INimbusMessageSender
    {
        private readonly ISerializer _serializer;
        private readonly Queue _queue;
        private readonly InProcessMessageStore _messageStore;
        private readonly ILogger _logger;

        public InProcessQueueSender(ISerializer serializer, Queue queue, InProcessMessageStore messageStore, ILogger logger)
        {
            _serializer = serializer;
            _queue = queue;
            _messageStore = messageStore;
            _logger = logger;
        }

        public async Task Send(NimbusMessage message)
        {
            var messageClone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
            AsyncBlockingCollection<NimbusMessage> messageQueue;
            if (!_messageStore.TryGetExistingMessageQueue(_queue.QueuePath, out messageQueue))
            {
                _logger.Warn("A message was sent to an in-process queue {QueuePath} but nobody is listening on that queue. Not sending.", _queue.QueuePath);
                return;
            }
            await messageQueue.Add(messageClone);
        }
    }
}