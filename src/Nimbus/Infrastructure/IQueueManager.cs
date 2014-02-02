using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal interface IQueueManager
    {
        //FIXME not sure that this belongs here. It doesn't actually need to know about Azure...
        INimbusMessageSender GetMessageSender(Type messageType);

        MessageSender CreateMessageSender(string queuePath);
        MessageReceiver CreateMessageReceiver(string queuePath);

        SubscriptionClient CreateSubscriptionReceiver(string topicPath, string subscriptionName);

        QueueClient CreateDeadLetterQueueClient<T>();
    }
}