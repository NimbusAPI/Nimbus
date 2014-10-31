using Microsoft.ServiceBus.Messaging;

namespace Nimbus.PropertyInjection
{
    public interface IRequireBrokeredMessage
    {
        BrokeredMessage BrokeredMessage { get; set; }
    }
}