using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal interface INimbusMessagingFactory
    {
        INimbusMessageSender GetQueueSender(string queuePath);
        INimbusMessageReceiver GetQueueReceiver(string queuePath);

        INimbusMessageSender GetTopicSender(string topicPath);
        INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName);
    }
}