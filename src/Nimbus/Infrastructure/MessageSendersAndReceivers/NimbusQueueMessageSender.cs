using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageSender : BatchingMessageSender
    {
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<MessageSender> _queueClient;

        public NimbusQueueMessageSender(IQueueManager queueManager, string queuePath, ILogger logger)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
            _logger = logger;

            _queueClient = new ThreadSafeLazy<MessageSender>(() => _queueManager.CreateMessageSender(_queuePath).Result);
        }

        protected override void SendBatch(BrokeredMessage[] toSend)
        {
            _logger.Debug("Flushing outbound message queue {0} ({1} messages)", _queuePath, toSend.Length);
            _queueClient.Value.SendBatch(toSend);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_queueClient.IsValueCreated) return;
            _queueClient.Value.Close();
        }
    }
}