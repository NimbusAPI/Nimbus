using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;

namespace Nimbus.Infrastructure.BrokeredMessageServices
{
    internal class BrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IClock _clock;
        private readonly ISerializer _serializer;
        private readonly ICompressor _compressor;
        private readonly IMessageBodyStore _messageBodyStore;

        public BrokeredMessageFactory(ReplyQueueNameSetting replyQueueName, ISerializer serializer, ICompressor compressor, IClock clock, IMessageBodyStore messageBodyStore)
        {
            _replyQueueName = replyQueueName;
            _serializer = serializer;
            _compressor = compressor;
            _clock = clock;
            _messageBodyStore = messageBodyStore;
        }

        public Task<BrokeredMessage> Create(object serializableObject = null)
        {
            return Task.Run(async () =>
                                  {
                                      BrokeredMessage message;
                                      if (serializableObject == null)
                                      {
                                          message = new BrokeredMessage();
                                      }
                                      else
                                      {
                                          const int sizeLimit = 64*1024; //FIXME we should grab this from the queue settings or somewhere.  -andrewh 7/4/2014
                                          var messageBodyBytes = BuildBodyBytes(serializableObject);

                                          if (messageBodyBytes.Length < sizeLimit)
                                          {
                                              message = new BrokeredMessage(new MemoryStream(messageBodyBytes), true);
                                          }
                                          else
                                          {
                                              message = new BrokeredMessage();
                                              var blobIdentifier = message.MessageId;
                                              message.Properties.Add(MessagePropertyKeys.LargeBodyBlobIdentifier, blobIdentifier);
                                              await _messageBodyStore.Store(blobIdentifier, messageBodyBytes, _clock.UtcNow.AddDays(367));   //FIXME if someone changes this afterwards then it'll hurt.
                                          }
                                      }

                                      message.ReplyTo = _replyQueueName;
                                      message.CorrelationId = message.MessageId; // Use the MessageId as a default CorrelationId

                                      if (serializableObject != null)
                                      {
                                          message.Properties[MessagePropertyKeys.MessageType] = serializableObject.GetType().FullName;
                                      }

                                      return message;
                                  });
        }

        public Task<BrokeredMessage> CreateSuccessfulResponse(object responseContent, BrokeredMessage originalRequest)
        {
            if (originalRequest == null) throw new ArgumentNullException("originalRequest");

            return Task.Run(async () =>
                                  {
                                      var responseMessage = (await Create(responseContent)).WithCorrelationId(originalRequest.CorrelationId);
                                      responseMessage.Properties[MessagePropertyKeys.RequestSuccessful] = true;

                                      return responseMessage;
                                  });
        }

        public Task<BrokeredMessage> CreateFailedResponse(BrokeredMessage originalRequest, Exception exception)
        {
            if (originalRequest == null) throw new ArgumentNullException("originalRequest");

            return Task.Run(async () =>
                                  {
                                      var responseMessage = (await Create()).WithCorrelationId(originalRequest.CorrelationId);
                                      responseMessage.Properties[MessagePropertyKeys.RequestSuccessful] = false;
                                      foreach (var prop in exception.ExceptionDetailsAsProperties(_clock.UtcNow)) responseMessage.Properties.Add(prop.Key, prop.Value);

                                      return responseMessage;
                                  });
        }

        private byte[] BuildBodyBytes(object serializableObject)
        {
            if (serializableObject == null) throw new ArgumentNullException("serializableObject");

            var serialized = _serializer.Serialize(serializableObject);
            var serializedBytes = Encoding.UTF8.GetBytes(serialized);
            var compressedBytes = _compressor.Compress(serializedBytes);
            return compressedBytes;
        }

        public Task<object> GetBody(BrokeredMessage message, Type type)
        {
            return Task.Run(async () =>
                                  {
                                      byte[] bodyBytes;

                                      object blobId;
                                      if (message.Properties.TryGetValue(MessagePropertyKeys.LargeBodyBlobIdentifier, out blobId))
                                      {
                                          bodyBytes = await _messageBodyStore.Retrieve((string) blobId);
                                      }
                                      else
                                      {
                                          // Yep, this will actually give us the body Stream instead of trying to deserialize the body... cool API bro!
                                          using (var dataStream = message.GetBody<Stream>())
                                          using (var memoryStream = new MemoryStream())
                                          {
                                              dataStream.CopyTo(memoryStream);
                                              bodyBytes = memoryStream.ToArray();
                                          }
                                      }

                                      var decompressedBytes = _compressor.Decompress(bodyBytes);
                                      var deserialized = _serializer.Deserialize(Encoding.UTF8.GetString(decompressedBytes), type);
                                      return deserialized;
                                  });
        }
    }
}