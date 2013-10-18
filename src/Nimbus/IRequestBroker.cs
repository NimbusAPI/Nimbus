namespace Nimbus
{
    public interface IRequestBroker
    {
        TBusResponse Handle<TBusRequest, TBusResponse>(TBusRequest request) where TBusRequest : BusRequest<TBusRequest, TBusResponse>;
    }
}