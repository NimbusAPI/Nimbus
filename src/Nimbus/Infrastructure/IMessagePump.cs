using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    public interface IMessagePump
    {
        Task Start();
        Task Stop();
    }
}