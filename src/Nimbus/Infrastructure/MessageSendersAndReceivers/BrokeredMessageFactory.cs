using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    public static class BrokeredMessageFactory
    {
        public static BrokeredMessage BuildMessage(NimbusMessage message)
        {
            var brokeredMessage = new BrokeredMessage(new MemoryStream(message.Payload), true);
            brokeredMessage.CorrelationId = message.CorrelationId.ToString();
            brokeredMessage.MessageId = message.MessageId.ToString();
            brokeredMessage.ReplyTo = message.ReplyTo;
            brokeredMessage.TimeToLive = message.TimeToLive;
            brokeredMessage.ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc;

            foreach (var property in message.Properties)
            {
                brokeredMessage.Properties.Add(property.Key, property.Value);
            }

            return brokeredMessage;
        }

        public static NimbusMessage BuildNimbusMessage(BrokeredMessage message)
        {
            return new NimbusMessage
            {
                MessageId = Guid.Parse(message.MessageId),
                CorrelationId = Guid.Parse(message.CorrelationId),
                ReplyTo = message.ReplyTo,
                TimeToLive = message.TimeToLive,
                ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc,
                Properties = (Dictionary<string, object>)message.Properties,

            };
        }
    }
}