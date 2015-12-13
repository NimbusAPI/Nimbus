using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal interface IBrokeredMessageFactory
    {
        BrokeredMessage BuildBrokeredMessage(NimbusMessage message);
        NimbusMessage BuildNimbusMessage(BrokeredMessage message);
    }
}