using System;

namespace Nimbus.PoisonMessages
{
    [Obsolete("Not quite sure what to do with this yet but it needs to be better than it is now.")]
    public class DeadLetterQueues : IDeadLetterQueues
    {
        private readonly IDeadLetterQueue _commandQueue;
        private readonly IDeadLetterQueue _requestQueue;

        public DeadLetterQueues(IDeadLetterQueue commandQueue, IDeadLetterQueue requestQueue)
        {
            _commandQueue = commandQueue;
            _requestQueue = requestQueue;
        }

        public IDeadLetterQueue CommandQueue
        {
            get { return _commandQueue; }
        }

        public IDeadLetterQueue RequestQueue
        {
            get { return _requestQueue; }
        }
    }
}