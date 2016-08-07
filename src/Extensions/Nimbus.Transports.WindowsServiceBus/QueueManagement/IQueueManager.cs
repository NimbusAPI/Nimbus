using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Filtering.Conditions;

namespace Nimbus.Transports.WindowsServiceBus.QueueManagement
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