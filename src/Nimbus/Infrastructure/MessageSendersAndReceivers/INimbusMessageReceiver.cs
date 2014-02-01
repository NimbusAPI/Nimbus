using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    public interface INimbusMessageReceiver
    {
        Task<BrokeredMessage> Receive();
    }
}