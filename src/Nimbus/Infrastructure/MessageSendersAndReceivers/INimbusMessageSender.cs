using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    public interface INimbusMessageSender
    {
        void Send(BrokeredMessage message);
    }
}