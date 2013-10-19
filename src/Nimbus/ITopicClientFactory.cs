using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public interface ITopicClientFactory
    {
        TopicClient GetTopicClient(Type busEventType);
    }
}