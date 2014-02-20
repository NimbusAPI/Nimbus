using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleCompetingEvent<TBusEvent> where TBusEvent : IBusEvent
    {
        void Handle(TBusEvent busEvent);
    }
}