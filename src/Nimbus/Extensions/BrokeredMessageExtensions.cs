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

        public static BrokeredMessage WithCorrelationId(this BrokeredMessage message, Guid correlationId)
        {
            message.CorrelationId = correlationId.ToString();
            return message;
        }

        public static BrokeredMessage WithCorrelationId(this BrokeredMessage message, string correlationId)
        {
            message.CorrelationId = correlationId;
            return message;
        }

        public static BrokeredMessage WithReplyTo(this BrokeredMessage message, string replyTo)
        {
            message.ReplyTo = replyTo;
            return message;
        }

        public static BrokeredMessage WithTimeToLive(this BrokeredMessage message, TimeSpan timeToLive)
        {
            message.TimeToLive = timeToLive;
            return message;
        }

        public static BrokeredMessage WithScheduledEnqueueTime(this BrokeredMessage message, DateTimeOffset scheduledEnqueueTimeOffset)
        {
            message.ScheduledEnqueueTimeUtc = scheduledEnqueueTimeOffset.UtcDateTime;
            return message;
        }

        public static BrokeredMessage WithRequestTimeout(this BrokeredMessage message, TimeSpan timeout)
        {
            message.Properties.Add(MessagePropertyKeys.RequestTimeoutInMilliseconds, (int)timeout.TotalMilliseconds);
            return message;
        }

        public static TimeSpan GetRequestTimeout(this BrokeredMessage message)
        {
            var requestTimeoutInMilliseconds = (int)message.Properties[MessagePropertyKeys.RequestTimeoutInMilliseconds];
            var timeout = TimeSpan.FromMilliseconds(requestTimeoutInMilliseconds);
            return timeout;
        }
    }
}