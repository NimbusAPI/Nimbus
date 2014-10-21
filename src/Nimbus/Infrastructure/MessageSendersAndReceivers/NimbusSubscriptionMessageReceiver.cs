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
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private SubscriptionClient _subscriptionClient;

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
                return messages.ToArray();
            }
            catch (Exception exc)
            {
                if (exc.IsTransientFault()) throw;
                DiscardSubscriptionClient();
                throw;
            }
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
            if (subscriptionClient.IsClosed) return;

            subscriptionClient.Close();
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