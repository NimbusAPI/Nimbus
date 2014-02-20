using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface ICommandBroker
    {
        void Dispatch<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand;
    }
}