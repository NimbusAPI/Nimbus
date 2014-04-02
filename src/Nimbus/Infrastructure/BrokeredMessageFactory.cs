using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure
{
    public class BrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IClock _clock;

        public BrokeredMessageFactory(ReplyQueueNameSetting replyQueueName, IClock clock)
        {
            _replyQueueName = replyQueueName;
            _clock = clock;
        }

        public BrokeredMessage Create(object serializableObject = null)
        {
            var message = serializableObject != null ? new BrokeredMessage(serializableObject) : new BrokeredMessage();
            message.ReplyTo = _replyQueueName;
            message.CorrelationId = message.MessageId; // Use the MessageId as a default CorrelationId

            if (serializableObject != null)
            {
                message.Properties[MessagePropertyKeys.MessageType] = serializableObject.GetType().FullName;
            }

            return message;
        }

        public BrokeredMessage CreateSuccessfulResponse(object responseContent, BrokeredMessage originalRequest)
        {
            if (originalRequest == null) throw new ArgumentNullException("originalRequest");
            var responseMessage = Create(responseContent).WithCorrelationId(originalRequest.CorrelationId);
            responseMessage.Properties[MessagePropertyKeys.RequestSuccessful] = true;

            return responseMessage;
        }

        public BrokeredMessage CreateFailedResponse(BrokeredMessage originalRequest, Exception exception)
        {
            if (originalRequest == null) throw new ArgumentNullException("originalRequest");
            var responseMessage = Create().WithCorrelationId(originalRequest.CorrelationId);
            responseMessage.Properties[MessagePropertyKeys.RequestSuccessful] = false;
            foreach (var prop in exception.ExceptionDetailsAsProperties(_clock.UtcNow)) responseMessage.Properties.Add(prop.Key, prop.Value);
            
            return responseMessage;
        }
    }
}