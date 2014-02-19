using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageReceiver : INimbusMessageReceiver
    {
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;
        private readonly BatchReceiveTimeoutSetting _batchReceiveTimeout;

        private readonly Lazy<MessageReceiver> _messageReceiver;

        public NimbusQueueMessageReceiver(IQueueManager queueManager, string queuePath, BatchReceiveTimeoutSetting batchReceiveTimeout)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
            _batchReceiveTimeout = batchReceiveTimeout;

            _messageReceiver = new Lazy<MessageReceiver>(CreateMessageReceiver, LazyThreadSafetyMode.PublicationOnly);
        }

        private MessageReceiver CreateMessageReceiver()
        {
            return _queueManager.CreateMessageReceiver(_queuePath);
        }

        public Task WaitUntilReady()
        {
            return Task.Run(() => { var dummy = _messageReceiver.Value; });
        }

        public Task<IEnumerable<BrokeredMessage>> Receive(int batchSize)
        {
            return _messageReceiver.Value.ReceiveBatchAsync(batchSize, _batchReceiveTimeout);
        }

        public override string ToString()
        {
            return _queuePath;
        }

        public void Dispose()
        {
            if (!_messageReceiver.IsValueCreated) return;
            _messageReceiver.Value.Close();
        }
    }
}