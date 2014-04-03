using System;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure
{
    public class BrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly GzipMessageCompressionSetting _gzipCompression;
        private readonly IClock _clock;

        public BrokeredMessageFactory(ReplyQueueNameSetting replyQueueName, GzipMessageCompressionSetting gzipCompression, IClock clock)
        {
            _replyQueueName = replyQueueName;
            _gzipCompression = gzipCompression;
            _clock = clock;
        }

        public BrokeredMessage Create(object serializableObject = null)
        {
            // We need to serialize the message ourselves if we want to be in control of the BrokeredMessage.BodyStream
            var message = new BrokeredMessage(serializableObject); // It's safe to pass the ctor a null
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