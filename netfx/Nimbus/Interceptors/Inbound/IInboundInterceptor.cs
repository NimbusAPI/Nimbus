using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.MessageContracts;

namespace Nimbus.Interceptors.Inbound
{
    public interface IInboundInterceptor
    {
        int Priority { get; }

        Task OnCommandHandlerExecuting<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
            where TBusCommand : IBusCommand;

        Task OnCommandHandlerSuccess<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage)
            where TBusCommand : IBusCommand;

        Task OnCommandHandlerError<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage, Exception exception)
            where TBusCommand : IBusCommand;

        Task OnRequestHandlerExecuting<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnRequestHandlerSuccess<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnRequestHandlerError<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage, Exception exception)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnMulticastRequestHandlerExecuting<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastRequestHandlerSuccess<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnMulticastRequestHandlerError<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, NimbusMessage nimbusMessage, Exception exception)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnEventHandlerExecuting<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage)
            where TBusEvent : IBusEvent;

        Task OnEventHandlerSuccess<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage)
            where TBusEvent : IBusEvent;

        Task OnEventHandlerError<TBusEvent>(TBusEvent busEvent, NimbusMessage nimbusMessage, Exception exception)
            where TBusEvent : IBusEvent;
    }
}