namespace Nimbus
{
    public interface IHandleRequest<TBusRequest, TBusResponse> where TBusRequest: BusRequest<TBusRequest, TBusResponse>
    {
        TBusResponse Handle(TBusRequest request);
    }
}