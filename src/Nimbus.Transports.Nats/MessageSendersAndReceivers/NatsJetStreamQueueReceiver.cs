using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal class NatsJetStreamQueueReceiver : NatsJetStreamMessageReceiver
    {
        private readonly string _queuePath;

        protected override string StreamName { get; }
        protected override string Subject => _queuePath;
        protected override string ConsumerName { get; }

        public NatsJetStreamQueueReceiver(string queuePath,
                                          NatsJetStreamContextFactory jsContextFactory,
                                          ISerializer serializer,
                                          ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                          IGlobalHandlerThrottle globalHandlerThrottle,
                                          ILogger logger)
            : base(jsContextFactory, serializer, concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queuePath = queuePath;
            StreamName = $"Q_{SanitiseName(queuePath)}";
            ConsumerName = SanitiseName(queuePath);
        }
    }
}
