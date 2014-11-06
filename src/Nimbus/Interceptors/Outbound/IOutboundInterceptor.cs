using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors.Outbound
{
    public interface IOutboundInterceptor
    {
        int Priority { get; }

        Task OnCommandSending<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
            where TBusCommand : IBusCommand;

        Task OnCommandSent<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage)
            where TBusCommand : IBusCommand;

        Task OnCommandSendingError<TBusCommand>(TBusCommand busCommand, BrokeredMessage brokeredMessage, Exception exception)
            where TBusCommand : IBusCommand;

        Task OnRequestSending<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnRequestSent<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnRequestSendingError<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage, Exception exception)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnResponseSending<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
            where TBusResponse : IBusResponse;

        Task OnResponseSent<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
            where TBusResponse : IBusResponse;

        Task OnResponseSendingError<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage, Exception exception)
            where TBusResponse : IBusResponse;

        Task OnMulticastRequestSending<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastRequestSent<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastRequestSendingError<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage, Exception exception)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastResponseSending<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastResponseSent<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastResponseSendingError<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage, Exception exception)
            where TBusResponse : IBusMulticastResponse;

        Task OnEventPublishing<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage)
            where TBusEvent : IBusEvent;

        Task OnEventPublished<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage)
            where TBusEvent : IBusEvent;

        Task OnEventPublishingError<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage, Exception exception)
            where TBusEvent : IBusEvent;
    }
}