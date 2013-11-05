using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IRequestBroker
    {
        TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse;
    }
}