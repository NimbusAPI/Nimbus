using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleUniqueEvent<TBusEvent> where TBusEvent : IBusEvent
    {
        void HandleUnique(TBusEvent busEvent);
    }
}