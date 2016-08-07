using System;
using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Transports.InProcess.QueueManagement;

namespace Nimbus.Transports.InProcess.DelayedDelivery
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

        public Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            // Deliberately not awaiting this task. We want it to run in the background.
            Task.Run(async () =>
                           {
                               var delay = deliveryTime.Subtract(_clock.UtcNow);
                               if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
                               await Task.Delay(delay);

                               var messageQueuePath = DeriveMessageQueuePath(message);

                               AsyncBlockingCollection<NimbusMessage> queue;
                               if (!_messageStore.TryGetExistingMessageQueue(messageQueuePath, out queue)) return;
                               await queue.Add(message);
                           }).ConfigureAwaitFalse();

            return Task.Delay(0);
        }

        private static string DeriveMessageQueuePath(NimbusMessage message)
        {
            object subscriptionName;
            if (message.Properties.TryGetValue(MessagePropertyKeys.RedeliveryToSubscriptionName, out subscriptionName))
            {
                return InProcessTransport.FullyQualifiedSubscriptionPath(message.DeliverTo, (string) subscriptionName);
            }

            return message.DeliverTo;
        }
    }
}