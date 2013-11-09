using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    internal interface ITopicClientFactory
    {
        TopicClient GetTopicClient(Type busEventType);
    }
}