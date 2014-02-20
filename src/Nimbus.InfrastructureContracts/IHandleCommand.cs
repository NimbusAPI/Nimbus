using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts
{
    public interface IHandleCommand<TBusCommand> where TBusCommand : IBusCommand
    {
        void Handle(TBusCommand busCommand);
    }
}