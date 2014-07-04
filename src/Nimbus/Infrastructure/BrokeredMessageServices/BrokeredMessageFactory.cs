using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.BrokeredMessageServices
{
    internal class BrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly MaxLargeMessageSizeSetting _maxLargeMessageSize;
        private readonly MaxSmallMessageSizeSetting _maxSmallMessageSize;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly IClock _clock;
        private readonly ICompressor _compressor;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly ILargeMessageBodyStore _largeMessageBodyStore;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;
        private readonly ISerializer _serializer;
        private readonly ITypeProvider _typeProvider;

        public BrokeredMessageFactory(MaxLargeMessageSizeSetting maxLargeMessageSize,
                                      MaxSmallMessageSizeSetting maxSmallMessageSize,
                                      ReplyQueueNameSetting replyQueueName,
                                      IClock clock,
                                      ICompressor compressor,
                                      IDependencyResolver dependencyResolver,
                                      IDispatchContextManager dispatchContextManager,
                                      ILargeMessageBodyStore largeMessageBodyStore,
                                      IOutboundInterceptorFactory outboundInterceptorFactory,
                                      ISerializer serializer,
                                      ITypeProvider typeProvider)
        {
            _maxLargeMessageSize = maxLargeMessageSize;
            _maxSmallMessageSize = maxSmallMessageSize;
            _replyQueueName = replyQueueName;
            _clock = clock;
            _compressor = compressor;
            _dependencyResolver = dependencyResolver;
            _dispatchContextManager = dispatchContextManager;
            _largeMessageBodyStore = largeMessageBodyStore;
            _outboundInterceptorFactory = outboundInterceptorFactory;
            _serializer = serializer;
            _typeProvider = typeProvider;
        }

        public Task<BrokeredMessage> Create(object serializableObject = null)
        {
            return Task.Run(async () =>
                                  {
                                      var currentDispatchContext = _dispatchContextManager.GetCurrentDispatchContext();
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
                                              var blobIdentifier = await _largeMessageBodyStore.Store(brokeredMessage.MessageId, messageBodyBytes, _clock.UtcNow.AddDays(367));
                                              brokeredMessage.Properties.Add(MessagePropertyKeys.LargeBodyBlobIdentifier, blobIdentifier);
                                              //FIXME source this timeout from somewhere more sensible.  -andrewh 8/4/2014
                                          }
                                          else
                                          {
                                              brokeredMessage = new BrokeredMessage(new MemoryStream(messageBodyBytes), true);
                                          }

                                          brokeredMessage.Properties[MessagePropertyKeys.MessageType] = serializableObject.GetType().FullName;
                                      }

                                      brokeredMessage.ReplyTo = _replyQueueName;

                                      // Use the CorrelationId for the current dispatch, otherwise start a new CorrelationId using the message we're sending
                                      brokeredMessage.CorrelationId = currentDispatchContext.CorrelationId ?? brokeredMessage.MessageId;

                                      using (var scope = _dependencyResolver.CreateChildScope())
                                      {
                                          var interceptors = _outboundInterceptorFactory.CreateInterceptors(scope);
                                          foreach (var interceptor in interceptors)
                                          {
                                              await interceptor.Decorate(brokeredMessage, serializableObject);
                                          }
                                      }

                                      return brokeredMessage;
                                  });
        }

        public Task<BrokeredMessage> CreateSuccessfulResponse(object responseContent, BrokeredMessage originalRequest)
        {
            if (originalRequest == null) throw new ArgumentNullException("originalRequest");

            return Task.Run(async () =>
                                  {
                                      var responseMessage = (await Create(responseContent)).WithReplyToRequestId(originalRequest.MessageId);
                                      responseMessage.Properties[MessagePropertyKeys.RequestSuccessful] = true;

                                      return responseMessage;
                                  });
        }

        public Task<BrokeredMessage> CreateFailedResponse(BrokeredMessage originalRequest, Exception exception)
        {
            if (originalRequest == null) throw new ArgumentNullException("originalRequest");

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
            if (serializableObject == null) throw new ArgumentNullException("serializableObject");

            var serialized = _serializer.Serialize(serializableObject);
            var serializedBytes = Encoding.UTF8.GetBytes(serialized);
            var compressedBytes = _compressor.Compress(serializedBytes);
            return compressedBytes;
        }

        public Task<object> GetBody(BrokeredMessage message)
        {
            var bodyType = GetBodyType(message);

            return Task.Run(async () =>
                                  {
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
                                  });
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