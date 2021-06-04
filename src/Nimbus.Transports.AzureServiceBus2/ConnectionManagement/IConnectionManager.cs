namespace Nimbus.Transports.AzureServiceBus2.ConnectionManagement
{
    using Azure.Messaging.ServiceBus;

    public interface IConnectionManager
    {
        ServiceBusSender CreateMessageSender(string queuePath);
        ServiceBusReceiver CreateMessageReceiver(string queuePath, ServiceBusReceiveMode receiveMode);
        ServiceBusSender CreateTopicClient(string topicPath);
        ServiceBusProcessor CreateSubscriptionClient(string topicPath, string subscriptionName, ServiceBusReceiveMode receiveMode);
    }
}