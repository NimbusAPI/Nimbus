using System.Collections.Generic;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.HandlerFactories
{
    public interface IMulticastRequestHandlerFactory
    {
        OwnedComponent<IEnumerable<IHandleRequest<TBusRequest, TBusResponse>>> GetHandlers<TBusRequest, TBusResponse>()
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse;
    }
}