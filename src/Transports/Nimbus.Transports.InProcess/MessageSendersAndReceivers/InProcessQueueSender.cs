using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

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

        public Task Send(NimbusMessage message)
        {
            return Task.Run(() =>
                            {
                                var messageClone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
                                var messageQueue = _messageStore.GetMessageQueue(_queue.QueuePath);
                                messageQueue.Add(messageClone);
                            }).ConfigureAwaitFalse();
        }
    }
}