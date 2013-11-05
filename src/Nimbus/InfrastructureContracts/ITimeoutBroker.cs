using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface ITimeoutBroker
    {
        void Dispatch<TBusTimeout>(TBusTimeout busTimeout) where TBusTimeout : IBusTimeout;
    }
}