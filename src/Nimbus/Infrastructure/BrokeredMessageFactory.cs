using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure
{
    internal class BrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly GzipMessageCompressionSetting _gzipMessageCompression;
        private readonly IClock _clock;
        private readonly ISerializer _serializer;

        public BrokeredMessageFactory(ReplyQueueNameSetting replyQueueName, ISerializer serializer, GzipMessageCompressionSetting gzipMessageCompression, IClock clock)
        {
            _replyQueueName = replyQueueName;
            _serializer = serializer;
            _gzipMessageCompression = gzipMessageCompression;
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
            var compressedBytes = Compress(serializedBytes);
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

            var decompressedBytes = Decompress(bodyBytes);
            var deserialized = _serializer.Deserialize(Encoding.UTF8.GetString(decompressedBytes), type);
            return deserialized;
        }

        private byte[] Compress(byte[] input)
        {
            if (_gzipMessageCompression == false) return input;

            using (var outputStream = new MemoryStream())
            {
                // We need to close the compression stream before we can get the fully realized output
                using (var inputStream = new MemoryStream(input))
                using (var compressionStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    inputStream.CopyTo(compressionStream);
                }

                return outputStream.ToArray();
            }
        }

        private byte[] Decompress(byte[] input)
        {
            if (_gzipMessageCompression == false) return input;

            using (var inputStream = new MemoryStream(input))
            using (var decompressionStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                decompressionStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}