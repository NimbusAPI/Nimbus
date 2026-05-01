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
    }
}
