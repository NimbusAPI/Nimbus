using System.Collections.Generic;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.HandlerFactories
{
    public interface ICompetingEventHandlerFactory
    {
        OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> GetHandlers<TBusEvent>() where TBusEvent : IBusEvent;
    }
}