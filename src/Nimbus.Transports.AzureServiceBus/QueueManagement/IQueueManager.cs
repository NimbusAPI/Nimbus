using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Nimbus.InfrastructureContracts.Filtering.Conditions;

namespace Nimbus.Transports.AzureServiceBus.QueueManagement
{
    internal interface IQueueManager
    {
        Task<IMessageSender> CreateMessageSender(string queuePath);
        Task<IMessageReceiver> CreateMessageReceiver(string queuePath);

        Task<ITopicClient> CreateTopicSender(string topicPath);
        Task<ISubscriptionClient> CreateSubscriptionReceiver(string topicPath, string subscriptionName, IFilterCondition filterCondition);

        Task MarkQueueAsNonExistent(string queuePath);
        Task MarkTopicAsNonExistent(string topicPath);
        Task MarkSubscriptionAsNonExistent(string topicPath, string subscriptionName);

        Task<IMessageSender> CreateDeadQueueMessageSender();
        Task<IMessageReceiver> CreateDeadQueueMessageReceiver();

        Task<bool> TopicExists(string topicPath);
        Task<bool> QueueExists(string queuePath);
    }
}