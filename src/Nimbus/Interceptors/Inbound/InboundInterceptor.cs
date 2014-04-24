using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors.Inbound
{
    public abstract class InboundInterceptor : IInboundInterceptor
    {
        public virtual int Priority { get; protected set; }

#pragma warning disable 1998
        public virtual async Task OnCommandHandlerExecuting(IBusCommand busCommand, BrokeredMessage brokeredMessage)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        public virtual async Task OnCommandHandlerSuccess(IBusCommand busCommand, BrokeredMessage brokeredMessage)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        public virtual async Task OnCommandHandlerError(IBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
#pragma warning restore 1998
        {
        }
    }
}