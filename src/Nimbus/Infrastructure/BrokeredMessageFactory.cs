using System;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure
{
    internal class BrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IClock _clock;
        private readonly ISerializer _serializer;

        public BrokeredMessageFactory(ReplyQueueNameSetting replyQueueName, ISerializer serializer, IClock clock)
        {
            _replyQueueName = replyQueueName;
            _serializer = serializer;
            _clock = clock;
        }

        public BrokeredMessage Create(object serializableObject = null)
        {   
            BrokeredMessage message;
            if (serializableObject == null) 
            {
                message = new BrokeredMessage();
            }
            else
            {
                var stream = _serializer.Serialize(serializableObject);
                message = new BrokeredMessage(stream, true);
            }

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

        public object GetBody(BrokeredMessage message, Type messageType)
        {
            using (var stream = message.GetBody<Stream>())
            {
                return _serializer.Deserialize(stream, messageType);
            }
        }

        public object GetBody<T>(BrokeredMessage message)
        {
            return (T)GetBody(message, typeof (T));
        }
    }
}