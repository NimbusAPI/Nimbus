using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors.Inbound
{
    public abstract class InboundInterceptor : IInboundInterceptor
    {
        public virtual int Priority { get; protected set; }

        public virtual async Task OnCommandHandlerExecuting(IBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
        }

        public virtual async Task OnCommandHandlerSuccess(IBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
        }

        public virtual async Task OnCommandHandlerError(IBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
        {
        }
    }
}