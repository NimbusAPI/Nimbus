using System;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Transports.InProcess.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess
{
    internal class InProcessDelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly InProcessMessageStore _messageStore;
        private readonly IClock _clock;

        public InProcessDelayedDeliveryService(InProcessMessageStore messageStore, IClock clock)
        {
            _messageStore = messageStore;
            _clock = clock;
        }

        public Task DeliverAt(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            // Deliberately not awaiting this task. We want it to run in the background.
            Task.Run(async () =>
                           {
                               var delay = deliveryTime.Subtract(_clock.UtcNow);
                               if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
                               await Task.Delay(delay);
                               var queue = _messageStore.GetMessageQueue(message.ReceivedFromPath);
                               queue.Add(message);
                           }).ConfigureAwaitFalse();

            return Task.Delay(0);
        }
    }
}