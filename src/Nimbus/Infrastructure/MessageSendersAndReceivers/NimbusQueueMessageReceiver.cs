using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageReceiver : ThrottlingMessageReceiver
    {
        public const int TransientRetryLimit = 20;
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;
        private readonly ILogger _logger;

        private volatile MessageReceiver _messageReceiver;
        private readonly object _mutex = new object();
        private int _transientErrorCount;

        public NimbusQueueMessageReceiver(IQueueManager queueManager, string queuePath, ConcurrentHandlerLimitSetting concurrentHandlerLimit, ILogger logger)
            : base(concurrentHandlerLimit, logger)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
            _logger = logger;
            _transientErrorCount = 0;
        }

        public override string ToString()
        {
            return _queuePath;
        }

        protected override async Task WarmUp()
        {
            await GetMessageReceiver();
        }

        protected override async Task<BrokeredMessage[]> FetchBatch(int batchSize, Task cancellationTask)
        {
            if (batchSize < 1) return new BrokeredMessage[0];

            try
            {
                var messageReceiver = await GetMessageReceiver();

                var receiveTask = messageReceiver.ReceiveBatchAsync(batchSize, TimeSpan.FromSeconds(300));
                await Task.WhenAny(receiveTask, cancellationTask);                
                if (cancellationTask.IsCompleted) return new BrokeredMessage[0];
                
                var messages = await receiveTask;
                _transientErrorCount = 0;
                return messages.ToArray();
            }
            catch (Exception exc)
            {
                if (IsTransientFault(exc))
                {
                    _transientErrorCount++;
                    throw;
                }
                _transientErrorCount = 0;
                _logger.Warn("Discarding subscription client");
                DiscardMessageReceiver();
                throw;
            }
        }

        private bool IsTransientFault(Exception exc)
        {
            //See related issue https://github.com/NimbusAPI/Nimbus/issues/218 
            //If we permanently get the same transient exception, we should treat it as non transient and renew the client in an attempt to resolve the issue    
            return _transientErrorCount <= TransientRetryLimit && exc.IsTransientFault();
        }

        private async Task<MessageReceiver> GetMessageReceiver()
        {
            if (_messageReceiver != null) return _messageReceiver;

            _messageReceiver = await _queueManager.CreateMessageReceiver(_queuePath);
            _messageReceiver.PrefetchCount = ConcurrentHandlerLimit;
            return _messageReceiver;
        }

        private void DiscardMessageReceiver()
        {
            var messageReceiver = _messageReceiver;
            _messageReceiver = null;

            if (messageReceiver == null) return;
            try
            {
                if (messageReceiver.IsClosed) return;

                messageReceiver.Close();
            }
            catch (Exception ex)
            {
                _logger.Warn($"Renewing subscription client. Got error {ex.GetType().FullName} {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing) return;

                DiscardMessageReceiver();
            }
            catch (MessagingEntityNotFoundException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}