using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;

namespace Nimbus.Extensions
{
    public static class BrokeredMessageExtensions
    {
        public static string SafelyGetBodyTypeNameOrDefault(this BrokeredMessage message)
        {
            object name;
            return (message.Properties.TryGetValue(MessagePropertyKeys.MessageType, out name) ? (string) name : default(string));
        }

        public static BrokeredMessage WithCorrelationId(this BrokeredMessage message, Guid correlationId)
        {
            message.CorrelationId = correlationId.ToString("N");
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
            message.TimeToLive = timeout;
            message.Properties.Add(MessagePropertyKeys.RequestTimeoutInMilliseconds, (int)timeout.TotalMilliseconds);
            return message;
        }

        public static BrokeredMessage WithProperty(this BrokeredMessage message, string key, object value)
        {
            message.Properties.Add(key, value);
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