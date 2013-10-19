using System.Threading.Tasks;

namespace Nimbus
{
    public interface ICommandSender
    {
        Task Send<TBusCommand>(TBusCommand busCommand);
    }
}