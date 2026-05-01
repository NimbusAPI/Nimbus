using NATS.Client.Core;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal class NatsTopicReceiver : NatsMessageReceiver
    {
        private readonly NatsSubscription _subscription;

        protected override string Subject => _subscription.TopicPath;
        protected override string QueueGroup => _subscription.SubscriptionName;

        // Dedicated per-subscription subject for retries so that a failed handler
        // requeues only to its own subscription, not fan-out to all subscribers.
        private string RetrySubject => $"{_subscription.TopicPath}.{_subscription.SubscriptionName}.retry";

        public NatsTopicReceiver(NatsSubscription subscription,
                                  NatsConnectionFactory connectionFactory,
                                  ISerializer serializer,
                                  ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                  IGlobalHandlerThrottle globalHandlerThrottle,
                                  ILogger logger)
            : base(connectionFactory, serializer, concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _subscription = subscription;
        }

        protected override Task OnWarmingUp(NatsConnection connection, CancellationToken ct)
            => AddNatsSubscription(connection, RetrySubject, QueueGroup, ct);

        protected override async Task<NimbusMessage?> Fetch(CancellationToken cancellationToken)
        {
            var message = await base.Fetch(cancellationToken);
            if (message != null)
                message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = RetrySubject;
            return message;
        }
    }
}
