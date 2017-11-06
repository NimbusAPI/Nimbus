using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusSubscriptionMessageReceiver : ThrottlingMessageReceiver
    {
        public const int TransientRetryLimit = 20;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private readonly ILogger _logger;
        private SubscriptionClient _subscriptionClient;
        private int _transientErrorCount;

        public NimbusSubscriptionMessageReceiver(IQueueManager queueManager,
                                                 string topicPath,
                                                 string subscriptionName,
                                                 ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                                 ILogger logger)
            : base(concurrentHandlerLimit, logger)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _subscriptionName = subscriptionName;
            _logger = logger;
            _transientErrorCount = 0;
        }

        public override string ToString()
        {
            return "{0}/{1}".FormatWith(_topicPath, _subscriptionName);
        }

        protected override async Task WarmUp()
        {
            await GetSubscriptionClient();
        }

        protected override async Task<BrokeredMessage[]> FetchBatch(int batchSize, Task cancellationTask)
        {
            if (batchSize < 1)
                return new BrokeredMessage[0];

            try
            {
                var subscriptionClient = await GetSubscriptionClient();

                var receiveTask = subscriptionClient.ReceiveBatchAsync(batchSize, TimeSpan.FromSeconds(300));
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
                DiscardSubscriptionClient();
                throw;
            }
        }

        private bool IsTransientFault(Exception exc)
        {
            //See related issue https://github.com/NimbusAPI/Nimbus/issues/218 
            //If we permanently get the same transient exception, we should treat it as non transient and renew the client in an attempt to resolve the issue    
            return _transientErrorCount <= TransientRetryLimit && exc.IsTransientFault();
        }

        private async Task<SubscriptionClient> GetSubscriptionClient()
        {
            if (_subscriptionClient != null) return _subscriptionClient;

            _subscriptionClient = await _queueManager.CreateSubscriptionReceiver(_topicPath, _subscriptionName);
            _subscriptionClient.PrefetchCount = ConcurrentHandlerLimit;
            return _subscriptionClient;
        }

        private void DiscardSubscriptionClient()
        {            
            var subscriptionClient = _subscriptionClient;
            _subscriptionClient = null;
            if (subscriptionClient == null) return;

            try
            {                
                if (subscriptionClient.IsClosed) return;

                subscriptionClient.Close();
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

                DiscardSubscriptionClient();
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