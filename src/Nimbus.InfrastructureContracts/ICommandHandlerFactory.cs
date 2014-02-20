using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface ICommandHandlerFactory
    {
        OwnedComponent<IHandleCommand<TBusCommand>> GetHandler<TBusCommand>() where TBusCommand : IBusCommand;
    }
}