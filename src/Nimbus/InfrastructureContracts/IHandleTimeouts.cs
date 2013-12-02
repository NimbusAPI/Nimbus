using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleTimeouts<TBusTimeout> where TBusTimeout : IBusTimeout
    {
        void Timeout(TBusTimeout busTimeout);
    }
}