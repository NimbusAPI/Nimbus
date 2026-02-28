using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors.Outbound
{
    public interface IOutboundInterceptor
    {
        int Priority { get; }

        Task OnCommandSending<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
            where TBusCommand : IBusCommand;

        Task OnCommandSent<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
            where TBusCommand : IBusCommand;

        Task OnCommandSendingError<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage, Exception exception)
            where TBusCommand : IBusCommand;

        Task OnRequestSending<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnRequestSent<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnRequestSendingError<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage, Exception exception)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnResponseSending<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage)
            where TBusResponse : IBusResponse;

        Task OnResponseSent<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage)
            where TBusResponse : IBusResponse;

        Task OnResponseSendingError<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage, Exception exception)
            where TBusResponse : IBusResponse;

        Task OnMulticastRequestSending<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastRequestSent<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastRequestSendingError<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage, Exception exception)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastResponseSending<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage)
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastResponseSent<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage)
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastResponseSendingError<TBusResponse>(TBusResponse busResponse, NimbusMessage nimbusMessage, Exception exception)
            where TBusResponse : IBusMulticastResponse;

        Task OnEventPublishing<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage)
            where TBusEvent : IBusEvent;

        Task OnEventPublished<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage)
            where TBusEvent : IBusEvent;

        Task OnEventPublishingError<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage, Exception exception)
            where TBusEvent : IBusEvent;
    }
}