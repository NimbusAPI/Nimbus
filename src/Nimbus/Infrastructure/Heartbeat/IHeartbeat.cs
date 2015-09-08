using System.Threading.Tasks;

namespace Nimbus.Infrastructure.Heartbeat
{
    public interface IHeartbeat
    {
        Task Start();
        Task Stop();
    }
}