using System.Threading.Tasks;

namespace Nimbus.Infrastructure.Commands
{
    internal interface ICommandSender
    {
        Task Send<TBusCommand>(TBusCommand busCommand);
    }
}