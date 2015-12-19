using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;

namespace Nimbus.Transports.WindowsServiceBus
{
    internal interface IBrokeredMessageFactory
    {
        BrokeredMessage BuildBrokeredMessage(NimbusMessage message);
        NimbusMessage BuildNimbusMessage(BrokeredMessage message);
    }
}