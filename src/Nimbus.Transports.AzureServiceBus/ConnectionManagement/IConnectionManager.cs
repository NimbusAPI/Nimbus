using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Nimbus.Transports.AzureServiceBus.ConnectionManagement
{
    public interface IConnectionManager
    {
        IMessageSender CreateMessageSender(string queuePath);
        IMessageReceiver CreateMessageReceiver(string queuePath, ReceiveMode receiveMode);
        ITopicClient CreateTopicClient(string topicPath);
        ISubscriptionClient CreateSubscriptionClient(string topicPath, string subscriptionName, ReceiveMode receiveMode);
    }
}