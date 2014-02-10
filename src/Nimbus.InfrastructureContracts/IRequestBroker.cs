using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IRequestBroker
    {
        TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : IBusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse;
    }
}