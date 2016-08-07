using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Transports.InProcess.QueueManagement;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessSubscriptionReceiver : InProcessQueueReceiver
    {
        private readonly Subscription _subscription;
        private readonly InProcessMessageStore _messageStore;

        public InProcessSubscriptionReceiver(Subscription subscription,
                                             ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                             InProcessMessageStore messageStore,
                                             IGlobalHandlerThrottle globalHandlerThrottle,
                                             ILogger logger)
            : base(
                InProcessTransport.FullyQualifiedSubscriptionPath(subscription.TopicPath, subscription.SubscriptionName),
                concurrentHandlerLimit,
                messageStore,
                globalHandlerThrottle,
                logger)
        {
            _subscription = subscription;
            _messageStore = messageStore;
        }

        protected override Task WarmUp()
        {
            var topic = _messageStore.GetTopic(_subscription.TopicPath);
            topic.Subscribe(_subscription.SubscriptionName);

            return base.WarmUp();
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            var message = await base.Fetch(cancellationToken);
            message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscription.SubscriptionName;
            return message;
        }
    }
}