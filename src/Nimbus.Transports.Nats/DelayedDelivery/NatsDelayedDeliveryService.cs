using System;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Nats.DelayedDelivery
{
    internal class NatsDelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly IClock _clock;
        private readonly INimbusTransport _transport;
        private readonly ILogger _logger;

        public NatsDelayedDeliveryService(IClock clock, INimbusTransport transport, ILogger logger)
        {
            _clock = clock;
            _transport = transport;
            _logger = logger;
        }

        public Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            _logger.Debug("Enqueuing {MessageId} for re-delivery at {DeliverAt}", message.MessageId, deliveryTime);

            Task.Run(async () =>
            {
                var delay = deliveryTime.Subtract(_clock.UtcNow);
                if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
                await Task.Delay(delay);

                _logger.Debug("Re-delivering {MessageId} (attempt {Attempt})", message.MessageId, message.DeliveryAttempts.Length);
                var sender = _transport.GetQueueSender(message.DeliverTo);
                await sender.Send(message);
            }).ConfigureAwaitFalse();

            return Task.CompletedTask;
        }
    }
}
