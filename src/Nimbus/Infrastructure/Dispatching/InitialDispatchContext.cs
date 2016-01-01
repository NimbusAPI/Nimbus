using System;

namespace Nimbus.Infrastructure.Dispatching
{
    internal class InitialDispatchContext : IDispatchContext
    {
        private readonly Guid _dispatchId;
        private readonly Guid _correlationId;

        public InitialDispatchContext()
        {
            _dispatchId = Guid.NewGuid();
            _correlationId = Guid.NewGuid();
        }

        public Guid? ResultOfMessageId { get { return null; } }
        public Guid DispatchId { get { return _dispatchId; } }
        public Guid CorrelationId { get { return _correlationId; } }
    }
}