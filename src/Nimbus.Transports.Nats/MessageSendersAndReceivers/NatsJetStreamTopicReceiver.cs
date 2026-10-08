using NATS.Client.JetStream.Models;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal class NatsJetStreamTopicReceiver : NatsJetStreamMessageReceiver
    {
        private readonly NatsSubscription _subscription;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;

        protected override string StreamName { get; }
        protected override string Subject => _subscription.TopicPath;
        protected override string ConsumerName { get; }
        protected override StreamConfigRetention StreamRetention => StreamConfigRetention.Limits;

        // Per-instance multicast subscriptions are orphaned by every restart; let the server drop consumers that stop pulling.
        protected override TimeSpan? ConsumerInactiveThreshold => _autoDeleteOnIdle.Value;

        // Events are never scheduled ahead (retries use their own stream), so the topic stream only needs to
        // keep messages as long as a subscriber could still come back for them. Must match NatsJetStreamTopicSender.
        protected override TimeSpan? StreamMaxAge => _autoDeleteOnIdle.Value;

        // Dedicated per-subscription subject for retries so that a failed handler
        // requeues only to its own subscription, not fan-out to all subscribers.
        private string RetrySubject => $"{_subscription.TopicPath}.{SanitiseName(_subscription.SubscriptionName)}.retry";
        private string RetryStreamName => SanitiseName(RetrySubject);
        private string RetryConsumerName => ConsumerName + "_retry";

        public NatsJetStreamTopicReceiver(NatsSubscription subscription,
                                          NatsJetStreamContextFactory jsContextFactory,
                                          ISerializer serializer,
                                          AutoDeleteOnIdleSetting autoDeleteOnIdle,
                                          ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                          IGlobalHandlerThrottle globalHandlerThrottle,
                                          ILogger logger)
            : base(jsContextFactory, serializer, concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _subscription = subscription;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            StreamName = SanitiseName(subscription.TopicPath);
            ConsumerName = SanitiseName(subscription.SubscriptionName);
        }

        protected override async Task OnWarmingUp()
        {
            await _jsContextFactory.EnsureStreamAsync(RetryStreamName, RetrySubject, StreamConfigRetention.Workqueue);
            var retryConsumer = await _jsContextFactory.EnsureConsumerAsync(RetryStreamName, new ConsumerConfig
            {
                Name = RetryConsumerName,
                DurableName = RetryConsumerName,
                FilterSubject = RetrySubject,
                AckPolicy = ConsumerConfigAckPolicy.Explicit,
                DeliverPolicy = ConsumerConfigDeliverPolicy.All,
                InactiveThreshold = _autoDeleteOnIdle.Value,
            });
            RegisterAdditionalConsumer(retryConsumer);
        }

        protected override NimbusMessage OnMessageReceived(NimbusMessage message)
        {
            // Route retries to the per-subscription retry subject, not back to the topic.
            message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = RetrySubject;
            return message;
        }
    }
}
