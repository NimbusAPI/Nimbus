using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleTimeout<TBusTimeout> where TBusTimeout : IBusTimeout
    {
        void Timeout(TBusTimeout busTimeout);
    }
}