using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageSender : BatchingMessageSender
    {
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;

        private readonly ThreadSafeLazy<MessageSender> _queueClient;

        public NimbusQueueMessageSender(IQueueManager queueManager, string queuePath)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;

            _queueClient = new ThreadSafeLazy<MessageSender>(() => _queueManager.CreateMessageSender(_queuePath).Result);
        }

        protected override void SendBatch(BrokeredMessage[] toSend)
        {
            Console.WriteLine("Flushing outbound message queue {1} ({0} messages)", toSend.Length, _queuePath);
            _queueClient.Value.SendBatch(toSend);
        }

        public override void Dispose()
        {
            if (!_queueClient.IsValueCreated) return;
            _queueClient.Value.Close();
        }
    }
}