using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    public interface ICommandSender
    {
        Task Send<TBusCommand>(TBusCommand busCommand);
    }
}