using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleMulticastEvent<TBusEvent> where TBusEvent : IBusEvent
    {
        void Handle(TBusEvent busEvent);
    }
}