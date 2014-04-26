using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors.Inbound
{
    public abstract class InboundInterceptor : IInboundInterceptor
    {
        public virtual int Priority
        {
            get { return default(int); }
        }

#pragma warning disable 1998
        public virtual async Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
            where TBusCommand : IBusCommand
        {
        }

        public virtual async Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
            where TBusCommand : IBusCommand
        {
        }

        public virtual async Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
            where TBusCommand : IBusCommand
        {
        }

        public virtual async Task OnRequestHandlerExecuting<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
        }

        public virtual async Task OnRequestHandlerSuccess<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
        }

        public virtual async Task OnRequestHandlerError<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest,
                                                                                   BrokeredMessage brokeredMessage,
                                                                                   Exception exception)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
        }

        public virtual async Task OnCommandHandlerExecuting(IBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
        }

        public virtual async Task OnCommandHandlerSuccess(IBusCommand busCommand, BrokeredMessage brokeredMessage)
        {
        }

        public virtual async Task OnCommandHandlerError(IBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
        {
        }
#pragma warning restore 1998
    }
}