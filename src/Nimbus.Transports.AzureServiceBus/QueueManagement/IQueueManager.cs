using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Transports.AzureServiceBus.QueueManagement
{
    internal interface IQueueManager
    {
        Task<MessageSender> CreateMessageSender(string queuePath);
        Task<MessageReceiver> CreateMessageReceiver(string queuePath);

        Task<TopicClient> CreateTopicSender(string topicPath);
        Task<SubscriptionClient> CreateSubscriptionReceiver(string topicPath, string subscriptionName, IFilterCondition filterCondition);

        Task MarkQueueAsNonExistent(string queuePath);
        Task MarkTopicAsNonExistent(string topicPath);
        Task MarkSubscriptionAsNonExistent(string topicPath, string subscriptionName);

        Task<MessageSender> CreateDeadQueueMessageSender();
        Task<MessageReceiver> CreateDeadQueueMessageReceiver();

        Task<bool> TopicExists(string topicPath);
        Task<bool> QueueExists(string queuePath);
    }
}