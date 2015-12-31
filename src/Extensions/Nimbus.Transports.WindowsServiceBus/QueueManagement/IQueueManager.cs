using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Transports.WindowsServiceBus.QueueManagement
{
    internal interface IQueueManager
    {
        Task<MessageSender> CreateMessageSender(string queuePath);
        Task<MessageReceiver> CreateMessageReceiver(string queuePath);

        Task<TopicClient> CreateTopicSender(string topicPath);
        Task<SubscriptionClient> CreateSubscriptionReceiver(string topicPath, string subscriptionName);

        Task<MessageSender> CreateDeadQueueMessageSender();
        Task<MessageReceiver> CreateDeadQueueMessageReceiver();
    }
}