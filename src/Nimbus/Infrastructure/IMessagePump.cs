using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    internal interface IMessagePump
    {
        Task Start();
        Task Stop();
    }
}