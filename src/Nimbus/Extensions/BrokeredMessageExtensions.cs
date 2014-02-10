using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;

namespace Nimbus.Extensions
{
    public static class BrokeredMessageExtensions
    {
        public static Type GetBodyType(this BrokeredMessage message)
        {
            var bodyTypeName = (string) message.Properties[MessagePropertyKeys.MessageType];
            var bodyType = Type.GetType(bodyTypeName);
            return bodyType;
        }

        public static object GetBody(this BrokeredMessage message, Type messageType)
        {
            var getBodyOpenGenericMethod = typeof (BrokeredMessage).GetMethod("GetBody", new Type[0]);
            var getBodyMethod = getBodyOpenGenericMethod.MakeGenericMethod(messageType);
            var body = getBodyMethod.Invoke(message, null);
            return body;
        }
    }
}