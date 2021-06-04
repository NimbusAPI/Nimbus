namespace Nimbus.Transports.AzureServiceBus2.ConnectionManagement
{
    using Azure.Messaging.ServiceBus;
    using Nimbus.Configuration.Settings;

    public interface IConnectionManager
    {
        ServiceBusSender CreateMessageSender(string queuePath);
        ServiceBusReceiver CreateMessageReceiver(string queuePath, ServiceBusReceiveMode receiveMode, int preFetchCount);
        ServiceBusSender CreateTopicClient(string topicPath);
        ServiceBusProcessor CreateSubscriptionClient(
            string topicPath,
            string subscriptionName,
            ServiceBusReceiveMode receiveMode,
            int preFetchCount);
    }
}