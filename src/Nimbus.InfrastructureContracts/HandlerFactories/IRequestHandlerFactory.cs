using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.HandlerFactories
{
    public interface IRequestHandlerFactory
    {
        OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>> GetHandler<TBusRequest, TBusResponse>()
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;
    }
}