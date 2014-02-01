using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public interface INimbusMessageSender
    {
        void Send(BrokeredMessage message);
    }
}