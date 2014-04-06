using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusSubscriptionMessageReceiver : NimbusMessageReceiver
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

        protected override async Task CreateBatchReceiver()
        {
            _subscriptionClient = await _queueManager.CreateSubscriptionReceiver(_topicPath, _subscriptionName);
            _subscriptionClient.PrefetchCount = ConcurrentHandlerLimit;
        }

        protected override async Task<BrokeredMessage[]> FetchBatch(int batchSize)
        {
            var messages = await _subscriptionClient.ReceiveBatchAsync(batchSize, TimeSpan.FromSeconds(300));
            return messages.ToArray();
        }

        protected override void StopBatchReceiver()
        {
            var subscriptionClient = _subscriptionClient;
            if (subscriptionClient == null) return;
            try
            {
                subscriptionClient.Close();
            }
            catch (MessagingEntityNotFoundException)
            {
            }
        }
    }
}