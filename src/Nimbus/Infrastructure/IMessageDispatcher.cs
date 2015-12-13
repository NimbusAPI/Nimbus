using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    internal interface IMessageDispatcher
    {
        Task Dispatch(NimbusMessage message);
    }
}