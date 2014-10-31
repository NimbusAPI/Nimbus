using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.Dispatching
{
    internal class SubsequentDispatchContext : IDispatchContext
    {
        private readonly string _dispatchId;
        private readonly string _correlationId;
        private readonly string _resultOfMessageId;

        public SubsequentDispatchContext(BrokeredMessage brokeredMessage)
        {
            _dispatchId = Guid.NewGuid().ToString("N");
            _correlationId = brokeredMessage.CorrelationId;
            _resultOfMessageId = brokeredMessage.MessageId;
        }

        public string DispatchId
        {
            get { return _dispatchId; }
        }

        public string ResultOfMessageId
        {
            get { return _resultOfMessageId; }
        }

        public string CorrelationId
        {
            get { return _correlationId; }
        }
    }
}