using System;
using Nimbus.Extensions;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure
{
    public class DestinationPerMessageTypeRouter : IRouter
    {
        public string Route(Type messageType)
        {
            if (typeof (IBusCommand).IsAssignableFrom(messageType) || messageType.IsClosedTypeOf(typeof (IBusRequest<,>)))
            {
                return PathFactory.QueuePathFor(messageType);
            }

            if (typeof (IBusEvent).IsAssignableFrom(messageType) || messageType.IsClosedTypeOf(typeof (IBusMulticastRequest<,>)))
            {
                return PathFactory.TopicPathFor(messageType);
            }

            throw new Exception("Cannot build a route for the message type '{0}'.".FormatWith(messageType.FullName));
        }
    }
}