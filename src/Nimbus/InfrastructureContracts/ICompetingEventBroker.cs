using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface ICompetingEventBroker
    {
        void PublishCompeting<TBusEvent>(TBusEvent busEvent) where TBusEvent : IBusEvent;
    }
}