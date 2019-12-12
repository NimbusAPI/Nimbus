using System;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.Dispatching
{
    internal class SubsequentDispatchContext : IDispatchContext
    {
        private readonly Guid _resultOfMessageId;

        public SubsequentDispatchContext(NimbusMessage nimbusMessage)
        {
            DispatchId = Guid.NewGuid();
            CorrelationId = nimbusMessage.CorrelationId;
            _resultOfMessageId = nimbusMessage.MessageId;
        }

        public Guid DispatchId { get; }
        public Guid CorrelationId { get; }
        public Guid? ResultOfMessageId => _resultOfMessageId;
    }
}