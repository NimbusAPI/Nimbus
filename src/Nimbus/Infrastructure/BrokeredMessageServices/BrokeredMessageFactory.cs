using System;
using System.IO;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.BrokeredMessageServices
{
    internal class BrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IClock _clock;
        private readonly ISerializer _serializer;
        private readonly ICompressor _compressor;

        public BrokeredMessageFactory(
            ReplyQueueNameSetting replyQueueName,
            ISerializer serializer,
            ICompressor compressor,
            IClock clock)
        {
            _replyQueueName = replyQueueName;
            _serializer = serializer;
            _compressor = compressor;
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
                var messageBodyBytes = BuildBodyBytes(serializableObject);
                message = new BrokeredMessage(new MemoryStream(messageBodyBytes), ownsStream: true);
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

        private byte[] BuildBodyBytes(object serializableObject)
        {
            if (serializableObject == null) throw new ArgumentNullException("serializableObject");

            var serialized = _serializer.Serialize(serializableObject);
            var serializedBytes = Encoding.UTF8.GetBytes(serialized);
            var compressedBytes = _compressor.Compress(serializedBytes);
            return compressedBytes;
        }

        public object GetBody(BrokeredMessage message, Type type)
        {
            // Yep, this will actually give us the body Stream instead of trying to deserialize the body... cool API bro!
            byte[] bodyBytes;
            using (var dataStream = message.GetBody<Stream>())
            using (var memoryStream = new MemoryStream())
            {
                dataStream.CopyTo(memoryStream);
                bodyBytes = memoryStream.ToArray();
            }

            var decompressedBytes = _compressor.Decompress(bodyBytes);
            var deserialized = _serializer.Deserialize(Encoding.UTF8.GetString(decompressedBytes), type);
            return deserialized;
        }
    }
}