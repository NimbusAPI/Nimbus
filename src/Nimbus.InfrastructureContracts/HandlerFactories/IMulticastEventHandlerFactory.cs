using System.Collections.Generic;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.HandlerFactories
{
    public interface IMulticastEventHandlerFactory
    {
        OwnedComponent<IEnumerable<IHandleMulticastEvent<TBusEvent>>> GetHandlers<TBusEvent>() where TBusEvent : IBusEvent;
    }
}