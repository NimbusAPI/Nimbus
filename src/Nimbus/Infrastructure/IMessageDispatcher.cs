using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    internal interface IMessageDispatcher
    {
        Task Dispatch(BrokeredMessage message);
    }
}