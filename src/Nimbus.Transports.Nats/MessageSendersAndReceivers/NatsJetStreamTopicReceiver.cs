using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal class NatsJetStreamTopicReceiver : NatsJetStreamMessageReceiver
    {
        private readonly NatsSubscription _subscription;

        // Each subscription gets its own durable consumer on the shared topic stream.
        // Multiple instances with the same subscription name compete for messages within
        // that subscription group (fan-out across groups, competing within a group).
        protected override string StreamName { get; }
        protected override string Subject => _subscription.TopicPath;
        protected override string ConsumerName { get; }

        public NatsJetStreamTopicReceiver(NatsSubscription subscription,
                                          NatsJetStreamContextFactory jsContextFactory,
                                          ISerializer serializer,
                                          ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                          IGlobalHandlerThrottle globalHandlerThrottle,
                                          ILogger logger)
            : base(jsContextFactory, serializer, concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _subscription = subscription;
            StreamName = $"T_{SanitiseName(subscription.TopicPath)}";
            ConsumerName = SanitiseName(subscription.SubscriptionName);
        }

        protected override NimbusMessage OnMessageReceived(NimbusMessage message)
        {
            // Route retries back to the topic subject so JetStream captures and fans-out again.
            message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscription.TopicPath;
            return message;
        }
    }
}
