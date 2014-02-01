using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public interface INimbusMessageReceiver
    {
        Task<BrokeredMessage> Receive();
    }
}