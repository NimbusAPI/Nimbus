using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    public interface INimbusMessageReceiver
    {
        Task WaitUntilReady();
        Task<BrokeredMessage> Receive();
    }
}