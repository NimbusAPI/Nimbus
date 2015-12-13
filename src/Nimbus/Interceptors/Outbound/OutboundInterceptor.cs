using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors.Outbound
{
    public abstract class OutboundInterceptor : IOutboundInterceptor
    {
        public virtual int Priority
        {
            get { return int.MaxValue; }
        }

        public virtual async Task OnCommandSending<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage) where TBusCommand : IBusCommand
        {
        }

        public virtual async Task OnCommandSent<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage) where TBusCommand : IBusCommand
        {
        }

        public virtual async Task OnCommandSendingError<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage, Exception exception) where TBusCommand : IBusCommand
        {
        }

        public virtual async Task OnRequestSending<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
        }

        public virtual async Task OnRequestSent<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
        }

        public virtual async Task OnRequestSendingError<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest,
                                                                                   NimbusMessage nimbusMessage,
                                                                                   Exception exception)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
        }

        public virtual async Task OnResponseSending<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage) where TBusResponse : IBusResponse
        {
        }

        public virtual async Task OnResponseSent<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage) where TBusResponse : IBusResponse
        {
        }

        public virtual async Task OnResponseSendingError<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage, Exception exception)
            where TBusResponse : IBusResponse
        {
        }

        public virtual async Task OnMulticastRequestSending<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse
        {
        }

        public virtual async Task OnMulticastRequestSent<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse
        {
        }

        public virtual async Task OnMulticastRequestSendingError<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest,
                                                                                            NimbusMessage nimbusMessage,
                                                                                            Exception exception)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse
        {
        }

        public virtual async Task OnMulticastResponseSending<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage) where TBusResponse : IBusMulticastResponse
        {
        }

        public virtual async Task OnMulticastResponseSent<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage) where TBusResponse : IBusMulticastResponse
        {
        }

        public virtual async Task OnMulticastResponseSendingError<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage, Exception exception)
            where TBusResponse : IBusMulticastResponse
        {
        }

        public virtual async Task OnEventPublishing<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage) where TBusEvent : IBusEvent
        {
        }

        public virtual async Task OnEventPublished<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage) where TBusEvent : IBusEvent
        {
        }

        public virtual async Task OnEventPublishingError<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage, Exception exception) where TBusEvent : IBusEvent
        {
        }
    }
}