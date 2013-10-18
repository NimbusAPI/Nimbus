using System;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public static class BrokeredMessageExtensions
    {
        public static object GetBody(this BrokeredMessage message, Type messageType)
        {
            var getBodyOpenGenericMethod = typeof(BrokeredMessage).GetMethod("GetBody", new Type[0]);
            var getBodyMethod = getBodyOpenGenericMethod.MakeGenericMethod(messageType);
            var body = getBodyMethod.Invoke(message, null);
            return body;
        }
    }
}