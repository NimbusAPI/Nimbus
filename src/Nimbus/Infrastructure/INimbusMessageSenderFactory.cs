using System;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal interface INimbusMessageSenderFactory
    {
        [Obsolete("Rename to GetQueueSender")]
        INimbusMessageSender GetMessageSender(Type messageType);
        [Obsolete("Rename to GetQueueSender")]
        INimbusMessageSender GetMessageSender(string queuePath);

        INimbusMessageSender GetTopicSender(Type messageType);
        INimbusMessageSender GetTopicSender(string topicPath);
    }
}