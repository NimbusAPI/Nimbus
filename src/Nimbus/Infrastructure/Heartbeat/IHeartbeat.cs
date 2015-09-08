using System.Threading.Tasks;

namespace Nimbus.Infrastructure.Heartbeat
{
    internal interface IHeartbeat
    {
        Task Start();
        Task Stop();
    }
}