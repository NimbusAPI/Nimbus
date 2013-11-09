using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;

namespace Nimbus.PoisonMessages
{
    public class DeadLetterQueue : IDeadLetterQueue
    {
        private readonly IQueueManager _queueManager;

        internal DeadLetterQueue(IQueueManager queueManager)
        {
            _queueManager = queueManager;
        }

        public async Task<TBusMessageContract> Pop<TBusMessageContract>() where TBusMessageContract : class
        {
            var queueClient = _queueManager.CreateDeadLetterQueueClient<TBusMessageContract>();

            var result = await queueClient.ReceiveAsync(TimeSpan.Zero);
            if (result == null) return null;

            await result.CompleteAsync();
            return result.GetBody<TBusMessageContract>();
        }
    }
}