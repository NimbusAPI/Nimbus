namespace Nimbus.Transports.AzureServiceBus2.ConnectionManagement
{
    using Azure.Messaging.ServiceBus;
    using Nimbus.Configuration.Settings;

    public class ConnectionManager : IConnectionManager
    {
        private readonly ServiceBusClient _serviceBusClient;

        public ConnectionManager(ConnectionStringSetting connectionStringSetting)
        {
            this._serviceBusClient = new ServiceBusClient(connectionStringSetting);
        }

        public ServiceBusSender CreateMessageSender(string queuePath)
        {
            return this._serviceBusClient.CreateSender(queuePath);
        }

        public ServiceBusReceiver CreateMessageReceiver(string queuePath, ServiceBusReceiveMode receiveMode)
        {
            var options = new ServiceBusReceiverOptions()
                          {
                              ReceiveMode = receiveMode
                          };

            return this._serviceBusClient.CreateReceiver(queuePath, options);
        }

        public ServiceBusSender CreateTopicClient(string topicPath)
        {
            return this._serviceBusClient.CreateSender(topicPath);
        }

        public ServiceBusProcessor CreateSubscriptionClient(string topicPath, string subscriptionName, ServiceBusReceiveMode receiveMode)
        {
            var options = new ServiceBusProcessorOptions()
                          {
                              ReceiveMode = receiveMode
                          };
            
            return this._serviceBusClient.CreateProcessor(topicPath, subscriptionName, options);
        }
    }
}