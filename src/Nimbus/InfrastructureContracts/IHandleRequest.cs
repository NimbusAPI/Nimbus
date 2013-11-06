using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleRequest<TBusRequest, TBusResponse> where TBusRequest : BusRequest<TBusRequest, TBusResponse> where TBusResponse : IBusResponse
    {
        TBusResponse Handle(TBusRequest request);
    }
}