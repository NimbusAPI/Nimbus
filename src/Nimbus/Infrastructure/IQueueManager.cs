using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    internal interface IQueueManager
    {
        Task<MessageSender> CreateMessageSender(string queuePath);
        Task<MessageReceiver> CreateMessageReceiver(string queuePath);

        Task<TopicClient> CreateTopicSender(string topicPath);
        Task<SubscriptionClient> CreateSubscriptionReceiver(string topicPath, string subscriptionName);

        Task<QueueClient> CreateDeadLetterQueueClient<T>();
    }
}