namespace Nimbus.Transports.AzureServiceBus2.QueueManagement
{
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using Nimbus.Configuration.Settings;
    using Nimbus.InfrastructureContracts.Filtering.Conditions;

    internal interface IQueueManager
    {
        Task<ServiceBusSender> CreateMessageSender(string queuePath);
        Task<ServiceBusReceiver> CreateMessageReceiver(string queuePath, int preFetchCount);

        Task<ServiceBusSender> CreateTopicSender(string topicPath);
        Task<ServiceBusProcessor> CreateSubscriptionReceiver(
            string topicPath,
            string subscriptionName,
            IFilterCondition filterCondition,
            int preFetchCount);

        Task MarkQueueAsNonExistent(string queuePath);
        Task MarkTopicAsNonExistent(string topicPath);
        Task MarkSubscriptionAsNonExistent(string topicPath, string subscriptionName);

        Task<ServiceBusSender> CreateDeadQueueMessageSender();
        Task<ServiceBusReceiver> CreateDeadQueueMessageReceiver();

        Task<bool> TopicExists(string topicPath);
        Task<bool> QueueExists(string queuePath);
    }
}