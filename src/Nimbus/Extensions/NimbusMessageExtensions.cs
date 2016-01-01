using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Infrastructure;

namespace Nimbus.Extensions
{
    internal static class NimbusMessageExtensions
    {
        internal static NimbusMessage WithCorrelationId(this NimbusMessage message, Guid correlationId)
        {
            message.CorrelationId = correlationId;
            return message;
        }

        internal static NimbusMessage WithCorrelationId(this NimbusMessage message, string correlationId)
        {
            message.CorrelationId = Guid.Parse(correlationId);
            return message;
        }

        internal static NimbusMessage WithReplyToRequestId(this NimbusMessage message, Guid requestId)
        {
            message.InReplyToMessageId = requestId;
            return message;
        }

        internal static NimbusMessage DestinedForQueue(this NimbusMessage message, string queuePath)
        {
            return message.WithProperty(MessagePropertyKeys.SentToQueue, queuePath);
        }

        internal static NimbusMessage DestinedForTopic(this NimbusMessage message, string topicPath)
        {
            return message.WithProperty(MessagePropertyKeys.SentToTopic, topicPath);
        }

        internal static NimbusMessage WithReplyTo(this NimbusMessage message, string replyTo)
        {
            message.From = replyTo;
            return message;
        }

        internal static NimbusMessage WithTimeToLive(this NimbusMessage message, TimeSpan timeToLive)
        {
            message.ExpiresAfter = DateTimeOffset.UtcNow.Add(timeToLive);
            return message;
        }

        internal static NimbusMessage WithScheduledEnqueueTime(this NimbusMessage message, DateTimeOffset scheduledEnqueueTimeOffset)
        {
            message.DeliverAfter = scheduledEnqueueTimeOffset.UtcDateTime;
            return message;
        }

        internal static NimbusMessage WithRequestTimeout(this NimbusMessage message, TimeSpan timeout)
        {
            message.ExpiresAfter = DateTimeOffset.UtcNow.Add(timeout);
            message.Properties.Add(MessagePropertyKeys.RequestTimeoutInMilliseconds, (int) timeout.TotalMilliseconds);
            return message;
        }

        internal static NimbusMessage WithProperty(this NimbusMessage message, string key, object value)
        {
            message.Properties.Add(key, value);
            return message;
        }

        internal static TimeSpan GetRequestTimeout(this NimbusMessage message)
        {
            var requestTimeoutInMilliseconds = (int) message.Properties[MessagePropertyKeys.RequestTimeoutInMilliseconds];
            var timeout = TimeSpan.FromMilliseconds(requestTimeoutInMilliseconds);
            return timeout;
        }

        internal static IDictionary<string, object> ExtractProperties(this NimbusMessage NimbusMessage)
        {
            var properties = NimbusMessage.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return properties;
        }

        internal static string SafelyGetBodyTypeNameOrDefault(this NimbusMessage message)
        {
            return message.Payload != null
                ? message.Payload.GetType().FullName
                : null;
        }
    }
}