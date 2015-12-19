using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Transports.WindowsServiceBus
{
    internal class BrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly MaxLargeMessageSizeSetting _maxLargeMessageSize;
        private readonly MaxSmallMessageSizeSetting _maxSmallMessageSize;
        private readonly ICompressor _compressor;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly ILargeMessageBodyStore _largeMessageBodyStore;
        private readonly ISerializer _serializer;
        private readonly ITypeProvider _typeProvider;
        private readonly IClock _clock;

        public BrokeredMessageFactory(MaxLargeMessageSizeSetting maxLargeMessageSize,
                                      MaxSmallMessageSizeSetting maxSmallMessageSize,
                                      IClock clock,
                                      ICompressor compressor,
                                      IDispatchContextManager dispatchContextManager,
                                      ILargeMessageBodyStore largeMessageBodyStore,
                                      ISerializer serializer,
                                      ITypeProvider typeProvider)
        {
            _serializer = serializer;
            _maxLargeMessageSize = maxLargeMessageSize;
            _maxSmallMessageSize = maxSmallMessageSize;
            _compressor = compressor;
            _dispatchContextManager = dispatchContextManager;
            _largeMessageBodyStore = largeMessageBodyStore;
            _typeProvider = typeProvider;
            _clock = clock;
        }

        public BrokeredMessage BuildBrokeredMessage(NimbusMessage message)
        {
            var serializedNimbusMessage = _serializer.Serialize(message);
            var brokeredMessage = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(serializedNimbusMessage)), true);
            brokeredMessage.CorrelationId = message.CorrelationId.ToString();
            brokeredMessage.MessageId = message.MessageId.ToString();
            brokeredMessage.ReplyTo = message.ReplyTo;
            brokeredMessage.TimeToLive = message.ExpiresAfter.Subtract(DateTimeOffset.UtcNow);
            brokeredMessage.ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc;

            foreach (var property in message.Properties)
            {
                brokeredMessage.Properties.Add(property.Key, property.Value);
            }

            return brokeredMessage;
        }

        public Task<NimbusMessage> Create(object payload = null)
        {
            return Task.Run(async () =>
                                  {
                                      NimbusMessage nimbusMessage;
                                      if (payload == null)
                                      {
                                          nimbusMessage = new NimbusMessage();
                                      }
                                      else
                                      {
                                          var messageBodyBytes = BuildBodyBytes(payload);

                                          if (messageBodyBytes.Length > _maxLargeMessageSize)
                                          {
                                              var errorMessage =
                                                  "Message body size of {0} is larger than the permitted maximum of {1}. You need to change this in your bus configuration settings if you want to send messages this large."
                                                      .FormatWith(messageBodyBytes.Length, _maxLargeMessageSize.Value);
                                              throw new BusException(errorMessage);
                                          }

                                          if (messageBodyBytes.Length > _maxSmallMessageSize)
                                          {
                                              nimbusMessage = new NimbusMessage();
                                              var expiresAfter = nimbusMessage.ExpiresAfter;
                                              var blobIdentifier = await _largeMessageBodyStore.Store(nimbusMessage.MessageId, messageBodyBytes, expiresAfter);
                                              nimbusMessage.Properties[MessagePropertyKeys.LargeBodyBlobIdentifier] = blobIdentifier;
                                          }
                                          else
                                          {
                                              nimbusMessage = new NimbusMessage(messageBodyBytes);
                                          }
                                          nimbusMessage.Properties[MessagePropertyKeys.MessageType] = payload.GetType().FullName;
                                      }

                                      var currentDispatchContext = _dispatchContextManager.GetCurrentDispatchContext();
                                      nimbusMessage.Properties[MessagePropertyKeys.PrecedingMessageId] = currentDispatchContext.ResultOfMessageId;
                                      nimbusMessage.CorrelationId = currentDispatchContext.CorrelationId;
                                      nimbusMessage.ReplyTo = nimbusMessage.ReplyTo;

                                      return nimbusMessage;
                                  });
        }

        public NimbusMessage BuildNimbusMessage(BrokeredMessage message)
        {
            var nimbusMessage = (NimbusMessage) GetBody(message).Result; //FIXME ick. Make async.
            return nimbusMessage;
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
            var deserialized = _serializer.Deserialize(Encoding.UTF8.GetString(decompressedBytes), typeof (NimbusMessage));
            return deserialized;
        }
    }
}