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

        protected override string StreamName { get; }
        protected override string Subject => _subscription.TopicPath;
        protected override string ConsumerName { get; }
        protected override StreamConfigRetention StreamRetention => StreamConfigRetention.Limits;

        // Dedicated per-subscription subject for retries so that a failed handler
        // requeues only to its own subscription, not fan-out to all subscribers.
        private string RetrySubject => $"{_subscription.TopicPath}.{SanitiseName(_subscription.SubscriptionName)}.retry";
        private string RetryStreamName => $"Q_{SanitiseName(RetrySubject)}";
        private string RetryConsumerName => ConsumerName + "_retry";

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
            });
            StartConsumerLoop(retryConsumer);
        }

        protected override NimbusMessage OnMessageReceived(NimbusMessage message)
        {
            // Route retries to the per-subscription retry subject, not back to the topic.
            message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = RetrySubject;
            return message;
        }
    }
}
