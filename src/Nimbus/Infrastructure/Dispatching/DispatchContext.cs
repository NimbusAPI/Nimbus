using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.Dispatching
{
    internal class DispatchContext : IDispatchContext
    {
        public DispatchContext(BrokeredMessage brokeredMessage)
        {
            DispatchId = Guid.NewGuid().ToString("N");
            MessageId = brokeredMessage.MessageId;
            CorrelationId = brokeredMessage.CorrelationId;
        }

        public string DispatchId { get; private set; }
        public string MessageId { get; private set; }
        public string CorrelationId { get; private set; }
    }
}