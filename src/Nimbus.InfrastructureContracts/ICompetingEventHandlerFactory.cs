using System.Collections.Generic;
using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface ICompetingEventHandlerFactory
    {
        OwnedComponent<IEnumerable<IHandleCompetingEvent<TBusEvent>>> GetHandler<TBusEvent>() where TBusEvent : IBusEvent;
    }
}