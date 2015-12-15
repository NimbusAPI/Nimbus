using System;
using System.Threading.Tasks;
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
            Task.Run(async () =>
                           {
                               var delay = deliveryTime.Subtract(_clock.UtcNow);
                               await Task.Delay(delay);
                               var queue = _messageStore.GetMessageQueue(message.DeliverTo);
                               queue.Add(message);
                           });

            return Task.Delay(0);
        }
    }
}