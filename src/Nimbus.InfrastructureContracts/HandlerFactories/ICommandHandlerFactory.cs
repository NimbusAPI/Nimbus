using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.HandlerFactories
{
    public interface ICommandHandlerFactory
    {
        OwnedComponent<IHandleCommand<TBusCommand>> GetHandler<TBusCommand>() where TBusCommand : IBusCommand;
    }
}