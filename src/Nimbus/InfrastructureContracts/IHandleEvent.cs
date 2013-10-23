using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleEvent<TBusEvent> where TBusEvent : IBusEvent
    {
        void Handle(TBusEvent busEvent);

    }
}