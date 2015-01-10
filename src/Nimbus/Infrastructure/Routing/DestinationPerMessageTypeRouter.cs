using System;
using Nimbus.Extensions;
using Nimbus.Routing;

namespace Nimbus.Infrastructure.Routing
{
    public class DestinationPerMessageTypeRouter : IRouter
    {
        public string Route(Type messageType, QueueOrTopic queueOrTopic, IPathGenerator pathGenerator)
        {
            switch (queueOrTopic)
            {
                case QueueOrTopic.Queue:
                    return pathGenerator.QueuePathFor(messageType);
                case QueueOrTopic.Topic:
                    return pathGenerator.TopicPathFor(messageType);
                default:
                    throw new Exception("Cannot build a route for the message type '{0}'.".FormatWith(messageType.FullName));
            }
        }
    }
}