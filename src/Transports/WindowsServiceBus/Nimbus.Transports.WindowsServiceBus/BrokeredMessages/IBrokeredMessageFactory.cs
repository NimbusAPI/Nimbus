using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;

namespace Nimbus.Transports.WindowsServiceBus.BrokeredMessages
{
    internal interface IBrokeredMessageFactory
    {
        BrokeredMessage BuildBrokeredMessage(NimbusMessage message);
        NimbusMessage BuildNimbusMessage(BrokeredMessage message);
    }
}