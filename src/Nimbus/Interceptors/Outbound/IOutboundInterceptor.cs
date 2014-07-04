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

        Task OnRequestSending<TBusRequest, TBusResponse>(IBusRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;

        Task OnResponseSending<TBusResponse>(TBusResponse busResponse, BrokeredMessage brokeredMessage)
            where TBusResponse : IBusResponse;

        Task OnMulticastRequestSending<TBusRequest, TBusResponse>(IBusMulticastRequest<TBusRequest, TBusResponse> busRequest, BrokeredMessage brokeredMessage)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse;

        Task OnEventPublishing<TBusEvent>(TBusEvent busEvent, BrokeredMessage brokeredMessage)
            where TBusEvent : IBusEvent;
    }
}