using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IRequestHandlerFactory
    {
        OwnedComponent<IHandleRequest<TBusRequest, TBusResponse>> GetHandler<TBusRequest, TBusResponse>()
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;
    }
}