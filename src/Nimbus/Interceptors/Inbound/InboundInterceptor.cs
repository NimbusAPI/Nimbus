using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors.Inbound
{
    public abstract class InboundInterceptor : IInboundInterceptor
    {
        private static readonly Task CompletedTask = Task.FromResult(0);

        public virtual int Priority
        {
            get { return default(int); }
        }

#pragma warning disable 1998
        public virtual Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
            where TBusCommand : IBusCommand
        {
            return CompletedTask;
        }

        public virtual Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
            where TBusCommand : IBusCommand
        {
            return CompletedTask;
        }

        public virtual Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
            where TBusCommand : IBusCommand
        {
            return CompletedTask;
        }

        public virtual Task OnRequestHandlerExecuting<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            return CompletedTask;
        }

        public virtual Task OnRequestHandlerSuccess<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            return CompletedTask;
        }

        public virtual Task OnRequestHandlerError<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage, Exception exception)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            return CompletedTask;
        }

        public virtual Task OnMulticastRequestHandlerExecuting<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse
        {
            return CompletedTask;
        }

        public virtual Task OnMulticastRequestHandlerSuccess<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse
        {
            return CompletedTask;
        }

        public virtual Task OnMulticastRequestHandlerError<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage, Exception exception)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse
        {
            return CompletedTask;
        }

        public virtual Task OnEventHandlerExecuting<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage)
            where TBusEvent : IBusEvent
        {
            return CompletedTask;
        }

        public virtual Task OnEventHandlerSuccess<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage)
            where TBusEvent : IBusEvent
        {
            return CompletedTask;
        }

        public virtual Task OnEventHandlerError<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage, Exception exception)
            where TBusEvent : IBusEvent
        {
            return CompletedTask;
        }

#pragma warning restore 1998
    }
}