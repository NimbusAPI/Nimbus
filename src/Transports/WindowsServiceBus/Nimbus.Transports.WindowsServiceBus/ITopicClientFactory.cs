using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Transports.WindowsServiceBus
{
    [Obsolete("We should be using a dependency on some kind of INimbusEventSender", true)]
    internal interface ITopicClientFactory
    {
        TopicClient GetTopicClient(Type busEventType);
    }
}