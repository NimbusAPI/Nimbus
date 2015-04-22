using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.BrokeredMessageServices
{
    internal class BrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly DefaultMessageTimeToLiveSetting _timeToLive;
        private readonly MaxLargeMessageSizeSetting _maxLargeMessageSize;
        private readonly MaxSmallMessageSizeSetting _maxSmallMessageSize;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IClock _clock;
        private readonly ICompressor _compressor;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly ILargeMessageBodyStore _largeMessageBodyStore;
        private readonly ISerializer _serializer;
        private readonly ITypeProvider _typeProvider;

        public BrokeredMessageFactory(DefaultMessageTimeToLiveSetting timeToLive,
                                      MaxLargeMessageSizeSetting maxLargeMessageSize,
                                      MaxSmallMessageSizeSetting maxSmallMessageSize,
                                      ReplyQueueNameSetting replyQueueName,
                                      IClock clock,
                                      ICompressor compressor,
                                      IDispatchContextManager dispatchContextManager,
                                      ILargeMessageBodyStore largeMessageBodyStore,
                                      ISerializer serializer,
                                      ITypeProvider typeProvider)
        {
            _timeToLive = timeToLive;
            _maxLargeMessageSize = maxLargeMessageSize;
            _maxSmallMessageSize = maxSmallMessageSize;
            _replyQueueName = replyQueueName;
            _clock = clock;
            _compressor = compressor;
            _dispatchContextManager = dispatchContextManager;
            _largeMessageBodyStore = largeMessageBodyStore;
            _serializer = serializer;
            _typeProvider = typeProvider;
        }

        public Task<BrokeredMessage> Create(object serializableObject = null)
        {
            return Task.Run(async () =>
                                  {
                                      BrokeredMessage brokeredMessage;
                                      if (serializableObject == null)
                                      {
                                          brokeredMessage = new BrokeredMessage();
                                      }
                                      else
                                      {
                                          var messageBodyBytes = BuildBodyBytes(serializableObject);

                                          if (messageBodyBytes.Length > _maxLargeMessageSize)
                                          {
                                              var errorMessage =
                                                  "Message body size of {0} is larger than the permitted maximum of {1}. You need to change this in your bus configuration settings if you want to send messages this large."
                                                      .FormatWith(messageBodyBytes.Length, _maxLargeMessageSize.Value);
                                              throw new BusException(errorMessage);
                                          }

                                          if (messageBodyBytes.Length > _maxSmallMessageSize)
                                          {
                                              brokeredMessage = new BrokeredMessage();
                                              var expiresAfter = _clock.UtcNow.AddSafely(_timeToLive.Value);
                                              var blobIdentifier = await _largeMessageBodyStore.Store(brokeredMessage.MessageId, messageBodyBytes, expiresAfter);
                                              brokeredMessage.Properties[MessagePropertyKeys.LargeBodyBlobIdentifier] = blobIdentifier;
                                          }
                                          else
                                          {
                                              brokeredMessage = new BrokeredMessage(new MemoryStream(messageBodyBytes), true);
                                          }
                                          brokeredMessage.Properties[MessagePropertyKeys.MessageType] = serializableObject.GetType().FullName;
                                      }

                                      var currentDispatchContext = _dispatchContextManager.GetCurrentDispatchContext();
                                      brokeredMessage.Properties[MessagePropertyKeys.PrecedingMessageId] = currentDispatchContext.ResultOfMessageId;
                                      brokeredMessage.CorrelationId = currentDispatchContext.CorrelationId ?? brokeredMessage.MessageId;
                                      brokeredMessage.ReplyTo = _replyQueueName;

                                      return brokeredMessage;
                                  });
        }

        public Task<BrokeredMessage> CreateSuccessfulResponse(object responseContent, BrokeredMessage originalRequest)
        {
            return Task.Run(async () =>
                                  {
                                      var responseMessage = (await Create(responseContent)).WithReplyToRequestId(originalRequest.MessageId);
                                      responseMessage.Properties[MessagePropertyKeys.RequestSuccessful] = true;

                                      return responseMessage;
                                  });
        }

        public Task<BrokeredMessage> CreateFailedResponse(BrokeredMessage originalRequest, Exception exception)
        {
            return Task.Run(async () =>
                                  {
                                      var responseMessage = (await Create()).WithReplyToRequestId(originalRequest.MessageId);
                                      responseMessage.Properties[MessagePropertyKeys.RequestSuccessful] = false;
                                      foreach (var prop in exception.ExceptionDetailsAsProperties(_clock.UtcNow)) responseMessage.Properties.Add(prop.Key, prop.Value);

                                      return responseMessage;
                                  });
        }

        private byte[] BuildBodyBytes(object serializableObject)
        {
            var serialized = _serializer.Serialize(serializableObject);
            var serializedBytes = Encoding.UTF8.GetBytes(serialized);
            var compressedBytes = _compressor.Compress(serializedBytes);
            return compressedBytes;
        }

        public async Task<object> GetBody(BrokeredMessage message)
        {
            var bodyType = GetBodyType(message);

            byte[] bodyBytes;

            object blobId;
            if (message.Properties.TryGetValue(MessagePropertyKeys.LargeBodyBlobIdentifier, out blobId))
            {
                bodyBytes = await _largeMessageBodyStore.Retrieve((string) blobId);
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
            var deserialized = _serializer.Deserialize(Encoding.UTF8.GetString(decompressedBytes), bodyType);
            return deserialized;
        }

        public Type GetBodyType(BrokeredMessage message)
        {
            var typeName = message.SafelyGetBodyTypeNameOrDefault();
            var candidates = _typeProvider.AllMessageContractTypes().Where(t => t.FullName == typeName).ToArray();
            if (candidates.Any() == false)
                throw new Exception("The type '{0}' was not discovered by the type provider and cannot be loaded.".FormatWith(typeName));

            // The TypeProvider should not provide a list of duplicates
            return candidates.Single();
        }
    }
}