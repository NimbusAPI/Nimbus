using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public interface IMessageDispatcher
    {
        Task Dispatch(BrokeredMessage message);
    }
}