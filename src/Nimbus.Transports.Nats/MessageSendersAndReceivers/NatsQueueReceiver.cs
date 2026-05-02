using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal class NatsQueueReceiver : NatsMessageReceiver
    {
        private readonly string _queuePath;

        protected override string Subject => _queuePath;
        protected override string QueueGroup => _queuePath;

        public NatsQueueReceiver(string queuePath,
                                 NatsConnectionFactory connectionFactory,
                                 ISerializer serializer,
                                 ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                 IGlobalHandlerThrottle globalHandlerThrottle,
                                 ILogger logger)
            : base(connectionFactory, serializer, concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queuePath = queuePath;
        }
    }
}
