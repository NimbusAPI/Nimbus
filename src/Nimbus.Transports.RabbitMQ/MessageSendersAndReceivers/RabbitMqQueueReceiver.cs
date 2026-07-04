using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using RabbitMQ.Client;

namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    internal class RabbitMqQueueReceiver : RabbitMqReceiverBase
    {
        private readonly string _queuePath;

        public RabbitMqQueueReceiver(string queuePath,
                                      RabbitMqConnectionManager connectionManager,
                                      RabbitMqMessageConverter messageConverter,
                                      ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                      IGlobalHandlerThrottle globalHandlerThrottle,
                                      ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, connectionManager, messageConverter, logger)
        {
            _queuePath = queuePath;
        }

        protected override string ConsumeQueue => _queuePath;
        protected override string ReceiverDescription => $"queue {_queuePath}";

        protected override async Task DeclareTopologyAsync(IChannel channel)
        {
            await channel.QueueDeclareAsync(_queuePath, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(_queuePath, RabbitMqConnectionManager.DelayedExchangeName, routingKey: _queuePath);
        }
    }
}
