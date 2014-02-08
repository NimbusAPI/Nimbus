using System;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal interface INimbusMessageSenderFactory
    {
        INimbusMessageSender GetQueueSender(Type messageType);
        INimbusMessageSender GetQueueSender(string queuePath);

        INimbusMessageSender GetTopicSender(Type messageType);
        INimbusMessageSender GetTopicSender(string topicPath);
    }
}