using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Transports.WindowsServiceBus.BrokeredMessages
{
    internal interface IBrokeredMessageFactory
    {
        BrokeredMessage BuildBrokeredMessage(NimbusMessage message);
        Task<NimbusMessage> BuildNimbusMessage(BrokeredMessage message);
    }
}