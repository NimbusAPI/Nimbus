using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    [Obsolete]
    internal interface ITopicClientFactory
    {
        TopicClient GetTopicClient(Type busEventType);
    }
}