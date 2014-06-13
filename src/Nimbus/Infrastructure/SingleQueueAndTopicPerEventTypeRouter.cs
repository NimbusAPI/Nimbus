using System;
using Nimbus.Extensions;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure
{
    public class SingleQueueAndTopicPerEventTypeRouter : IRouter
    {
        private readonly string _routeAllCommandsToQueuePath;
        private readonly string _routeAllRequestsToQueuePath;

        public SingleQueueAndTopicPerEventTypeRouter(string routeAllCommandsToQueuePath, string routeAllRequestsToQueuePath)
        {
            _routeAllCommandsToQueuePath = routeAllCommandsToQueuePath;
            _routeAllRequestsToQueuePath = routeAllRequestsToQueuePath;
        }

        public string Route(Type messageType)
        {
            if (typeof (IBusCommand).IsAssignableFrom(messageType))
            {
                return _routeAllCommandsToQueuePath;
            }

            if (messageType.IsClosedTypeOf(typeof (IBusRequest<,>)))
            {
                return _routeAllRequestsToQueuePath;
            }

            if (typeof (IBusEvent).IsAssignableFrom(messageType) || messageType.IsClosedTypeOf(typeof (IBusMulticastRequest<,>)))
            {
                return PathFactory.TopicPathFor(messageType);
            }

            throw new Exception("Cannot build a route for the message type '{0}'.".FormatWith(messageType.FullName));
        }
    }
}