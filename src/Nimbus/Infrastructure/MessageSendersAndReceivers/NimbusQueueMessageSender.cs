using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageSender : INimbusMessageSender
    {
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;

        private readonly Lazy<MessageSender> _queueClient;
        readonly SemaphoreSlim _throttle = new SemaphoreSlim(10);

        public NimbusQueueMessageSender(IQueueManager queueManager, string queuePath)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;

            _queueClient = new Lazy<MessageSender>(() => _queueManager.CreateMessageSender(_queuePath));
        }

        public async Task Send(BrokeredMessage message)
        {
            _throttle.Wait();
            try
            {
                await _queueClient.Value.SendAsync(message);
            }
            finally
            {
                _throttle.Release();
            }
            
        }

        public void Dispose()
        {
            if (!_queueClient.IsValueCreated) return;
            _queueClient.Value.Close();
        }
    }
}