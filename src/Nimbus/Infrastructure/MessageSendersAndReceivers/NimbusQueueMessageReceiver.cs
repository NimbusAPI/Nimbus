using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageReceiver : NimbusMessageReceiver
    {
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;

        private MessageReceiver _messageReceiver;

        public NimbusQueueMessageReceiver(IQueueManager queueManager, string queuePath, ConcurrentHandlerLimitSetting concurrentHandlerLimit, ILogger logger)
            : base(concurrentHandlerLimit, logger)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
        }

        public override string ToString()
        {
            return _queuePath;
        }

        protected override async Task CreateBatchReceiver()
        {
            _messageReceiver = await _queueManager.CreateMessageReceiver(_queuePath);
            _messageReceiver.PrefetchCount = ConcurrentHandlerLimit;
        }

        protected override async Task<BrokeredMessage[]> FetchBatch(int batchSize)
        {
            if (_messageReceiver.IsClosed) return new BrokeredMessage[0];
            var messages = await _messageReceiver.ReceiveBatchAsync(batchSize, TimeSpan.FromSeconds(300));
            return messages.ToArray();
        }

        protected override void StopBatchReceiver()
        {
            var messageReceiver = _messageReceiver;
            if (messageReceiver == null) return;

            try
            {
                if (!messageReceiver.IsClosed) messageReceiver.Close();
            }
            catch (MessagingEntityNotFoundException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}