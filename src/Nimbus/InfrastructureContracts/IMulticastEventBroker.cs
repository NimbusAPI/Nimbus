using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IMulticastEventBroker
    {
        void PublishMulticast<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent;
    }
}