using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace Nimbus.InfrastructureContracts.Handlers
{
    public interface IHandleCommand<TBusCommand> where TBusCommand : IBusCommand
    {
        Task Handle(TBusCommand busCommand);
    }
}