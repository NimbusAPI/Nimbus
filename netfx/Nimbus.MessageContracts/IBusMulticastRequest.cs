namespace Nimbus.MessageContracts
{
    public interface IBusMulticastRequest<TBusRequest, TBusResponse> where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
                                                                     where TBusResponse : IBusMulticastResponse
    {
    }
}