using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    public interface IBrokeredMessageFactory
    {
        BrokeredMessage BuildBrokeredMessage(NimbusMessage message);
        NimbusMessage BuildNimbusMessage(BrokeredMessage message);
    }
}