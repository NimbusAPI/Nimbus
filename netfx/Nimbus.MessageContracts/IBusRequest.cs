namespace Nimbus.MessageContracts
{
    public interface IBusRequest<TRequest, TResponse> where TRequest : IBusRequest<TRequest, TResponse>
                                                      where TResponse : IBusResponse
    {
    }
}