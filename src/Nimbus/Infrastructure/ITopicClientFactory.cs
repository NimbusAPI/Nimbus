using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    public interface ITopicClientFactory
    {
        TopicClient GetTopicClient(Type busEventType);
    }
}