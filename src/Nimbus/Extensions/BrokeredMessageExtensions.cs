using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure;

namespace Nimbus.Extensions
{
    internal static class BrokeredMessageExtensions
    {
        internal static string SafelyGetBodyTypeNameOrDefault(this BrokeredMessage message)
        {
            object name;
            return (message.Properties.TryGetValue(MessagePropertyKeys.MessageType, out name) ? (string) name : default(string));
        }

        internal static BrokeredMessage WithCorrelationId(this BrokeredMessage message, Guid correlationId)
        {
            message.CorrelationId = correlationId.ToString("N");
            return message;
        }

        internal static BrokeredMessage WithCorrelationId(this BrokeredMessage message, string correlationId)
        {
            message.CorrelationId = correlationId;
            return message;
        }

        internal static BrokeredMessage WithReplyToRequestId(this BrokeredMessage message, string requestId)
        {
            return message.WithProperty(MessagePropertyKeys.InReplyToRequestId, requestId);
        }

        internal static BrokeredMessage DestinedForQueue(this BrokeredMessage message, string queuePath)
        {
            return message.WithProperty(MessagePropertyKeys.SentToQueue, queuePath);
        }

        internal static BrokeredMessage DestinedForTopic(this BrokeredMessage message, string topicPath)
        {
            return message.WithProperty(MessagePropertyKeys.SentToTopic, topicPath);
        }

        internal static BrokeredMessage WithReplyTo(this BrokeredMessage message, string replyTo)
        {
            message.ReplyTo = replyTo;
            return message;
        }

        internal static BrokeredMessage WithTimeToLive(this BrokeredMessage message, TimeSpan timeToLive)
        {
            message.TimeToLive = timeToLive;
            return message;
        }

        internal static BrokeredMessage WithScheduledEnqueueTime(this BrokeredMessage message, DateTimeOffset scheduledEnqueueTimeOffset)
        {
            message.ScheduledEnqueueTimeUtc = scheduledEnqueueTimeOffset.UtcDateTime;
            return message;
        }

        internal static BrokeredMessage WithRequestTimeout(this BrokeredMessage message, TimeSpan timeout)
        {
            message.TimeToLive = timeout;
            message.Properties.Add(MessagePropertyKeys.RequestTimeoutInMilliseconds, (int) timeout.TotalMilliseconds);
            return message;
        }

        internal static BrokeredMessage WithProperty(this BrokeredMessage message, string key, object value)
        {
            message.Properties.Add(key, value);
            return message;
        }

        internal static TimeSpan GetRequestTimeout(this BrokeredMessage message)
        {
            var requestTimeoutInMilliseconds = (int) message.Properties[MessagePropertyKeys.RequestTimeoutInMilliseconds];
            var timeout = TimeSpan.FromMilliseconds(requestTimeoutInMilliseconds);
            return timeout;
        }

        internal static IDictionary<string, object> ExtractProperties(this BrokeredMessage brokeredMessage)
        {
            var properties = brokeredMessage.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            properties[MessagePropertyKeys.MessageId] = brokeredMessage.MessageId;
            properties[MessagePropertyKeys.CorrelationId] = brokeredMessage.CorrelationId;
            return properties;
        }
    }
}