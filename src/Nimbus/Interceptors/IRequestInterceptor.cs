using Nimbus.MessageContracts;

namespace Nimbus.Interceptors
{
    public interface IRequestInterceptor<in TBusRequest, TBusResponse> : IMessageInterceptor<TBusRequest> where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
    {
    }
}