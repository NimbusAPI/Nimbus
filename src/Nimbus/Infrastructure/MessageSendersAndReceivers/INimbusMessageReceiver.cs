using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal interface INimbusMessageReceiver
    {
        Task WaitUntilReady();
        Task<BrokeredMessage> Receive();
    }
}