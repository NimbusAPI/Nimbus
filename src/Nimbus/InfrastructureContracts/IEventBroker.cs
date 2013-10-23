using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IEventBroker
    {
        void Publish<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent;
    }
}