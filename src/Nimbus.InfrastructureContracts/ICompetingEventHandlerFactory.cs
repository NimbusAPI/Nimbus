using System.Collections.Generic;
using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface ICompetingEventHandlerFactory
    {
        OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> GetHandlers<TBusEvent>() where TBusEvent : IBusEvent;
    }
}