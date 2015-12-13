using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.Dispatching
{
    internal class SubsequentDispatchContext : IDispatchContext
    {
        private readonly Guid _dispatchId;
        private readonly Guid _correlationId;
        private readonly Guid _resultOfMessageId;

        public SubsequentDispatchContext(NimbusMessage brokeredMessage)
        {
            _dispatchId = Guid.NewGuid();
            _correlationId = brokeredMessage.CorrelationId;
            _resultOfMessageId = brokeredMessage.MessageId;
        }

        public Guid DispatchId
        {
            get { return _dispatchId; }
        }

        public Guid? ResultOfMessageId
        {
            get { return _resultOfMessageId; }
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }
    }
}