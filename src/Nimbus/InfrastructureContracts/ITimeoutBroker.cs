using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface ITimeoutBroker
    {
        void Dispatch<TBusTimeout>(TBusTimeout busCommand) where TBusTimeout : IBusTimeout;
    }
}