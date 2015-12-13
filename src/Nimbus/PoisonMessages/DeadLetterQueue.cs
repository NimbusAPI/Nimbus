using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;

namespace Nimbus.PoisonMessages
{
    public class DeadLetterQueue : IDeadLetterQueue
    {
        private readonly IQueueManager _queueManager;
        private readonly INimbusMessageFactory _nimbusMessageFactory;

        internal DeadLetterQueue(INimbusMessageFactory nimbusMessageFactory, IQueueManager queueManager)
        {
            _queueManager = queueManager;
            _nimbusMessageFactory = nimbusMessageFactory;
        }

        public async Task<TBusMessageContract> Pop<TBusMessageContract>() where TBusMessageContract : class
        {
            //var queueClient = await _queueManager.CreateDeadLetterQueueClient<TBusMessageContract>();

            //var result = await queueClient.ReceiveAsync(TimeSpan.Zero);
            //if (result == null) return null;

            //await result.CompleteAsync();
            //return (TBusMessageContract) await _nimbusMessageFactory.GetBody(result);

            throw new NotImplementedException();
        }
    }
}