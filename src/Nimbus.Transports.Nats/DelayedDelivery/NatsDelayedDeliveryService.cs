using System;
using System.Threading.Tasks;
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

            _ = Task.Run(async () =>
            {
                try
                {
                    var delay = deliveryTime.Subtract(_clock.UtcNow);
                    if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
                    await Task.Delay(delay);

                    // For topic subscribers, route to the per-subscription retry subject so only
                    // the failing subscription receives the retry, not all subscribers.
                    var destination = message.Properties.TryGetValue(MessagePropertyKeys.RedeliveryToSubscriptionName, out var sub)
                        ? (string)sub!
                        : message.DeliverTo;

                    _logger.Debug("Re-delivering {MessageId} (attempt {Attempt}) to {Destination}", message.MessageId, message.DeliveryAttempts.Length, destination);
                    var sender = _transport.GetQueueSender(destination);
                    await sender.Send(message);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to re-deliver {MessageId} to {Destination}", message.MessageId, message.DeliverTo);
                }
            });

            return Task.CompletedTask;
        }
    }
}
