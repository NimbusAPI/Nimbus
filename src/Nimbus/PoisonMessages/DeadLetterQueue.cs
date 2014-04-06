using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;

namespace Nimbus.PoisonMessages
{
    public class DeadLetterQueue : IDeadLetterQueue
    {
        private readonly IQueueManager _queueManager;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;

        internal DeadLetterQueue(IQueueManager queueManager, IBrokeredMessageFactory brokeredMessageFactory)
        {
            _queueManager = queueManager;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task<TBusMessageContract> Pop<TBusMessageContract>() where TBusMessageContract : class
        {
            var queueClient = await _queueManager.CreateDeadLetterQueueClient<TBusMessageContract>();

            var result = await queueClient.ReceiveAsync(TimeSpan.Zero);
            if (result == null) return null;

            await result.CompleteAsync();
            return (TBusMessageContract)_brokeredMessageFactory.GetBody(result, typeof(TBusMessageContract));
        }
    }
}