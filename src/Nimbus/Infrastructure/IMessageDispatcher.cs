using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    public interface IMessageDispatcher
    {
        Task Dispatch(NimbusMessage message);
    }
}