using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Nimbus.Configuration.LargeMessages.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.MessageContracts;
using Nimbus.MessageContracts.Exceptions;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AzureServiceBus.Messages
{
    internal class BrokeredBrokeredMessageFactory : IBrokeredMessageFactory
    {
        private readonly MaxLargeMessageSizeSetting _maxLargeMessageSize;
        private readonly MaxSmallMessageSizeSetting _maxSmallMessageSize;
        private readonly ICompressor _compressor;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly ILargeMessageBodyStore _largeMessageBodyStore;
        private readonly ISerializer _serializer;
        private readonly ITypeProvider _typeProvider;
        private readonly IClock _clock;

        public BrokeredBrokeredMessageFactory(MaxLargeMessageSizeSetting maxLargeMessageSize,
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

        public Task<Message> BuildMessage(NimbusMessage message)
        {
            return Task.Run(async () =>
                                  {
                                      Message brokeredMessage;
                                      var messageBodyBytes = SerializeNimbusMessage(message);

                                      if (messageBodyBytes.Length > _maxLargeMessageSize)
                                      {
                                          var errorMessage =
                                              "Message body size of {0} is larger than the permitted maximum of {1}. You need to change this in your bus configuration settings if you want to send messages this large."
                                                  .FormatWith(messageBodyBytes.Length, _maxLargeMessageSize.Value);
                                          throw new BusException(errorMessage);
                                      }

                                      if (messageBodyBytes.Length > _maxSmallMessageSize)
                                      {
                                          brokeredMessage = new Message();
                                          var expiresAfter = message.ExpiresAfter;
                                          var blobIdentifier = await _largeMessageBodyStore.Store(message.MessageId, messageBodyBytes, expiresAfter);
                                          brokeredMessage.UserProperties[MessagePropertyKeys.LargeBodyBlobIdentifier] = blobIdentifier;
                                      }
                                      else
                                      {
                                          brokeredMessage = new Message(messageBodyBytes);
                                      }

                                      var currentDispatchContext = _dispatchContextManager.GetCurrentDispatchContext();
                                      brokeredMessage.MessageId = message.MessageId.ToString();
                                      brokeredMessage.CorrelationId = currentDispatchContext.CorrelationId.ToString();
                                      brokeredMessage.ReplyTo = message.From;
                                      brokeredMessage.TimeToLive = message.ExpiresAfter.Subtract(DateTimeOffset.UtcNow);
                                      brokeredMessage.ScheduledEnqueueTimeUtc = message.DeliverAfter.UtcDateTime;

                                      foreach (var property in message.Properties)
                                      {
                                          brokeredMessage.UserProperties[property.Key] = property.Value;
                                      }

                                      return brokeredMessage;
                                  });
        }

        public async Task<NimbusMessage> BuildNimbusMessage(Message message)
        {
            var nimbusMessage = await DeserializeNimbusMessage(message);
            return nimbusMessage;
        }

        private byte[] SerializeNimbusMessage(object serializableObject)
        {
            var serializedString = _serializer.Serialize(serializableObject);
            var serializedBytes = Encoding.UTF8.GetBytes(serializedString);
            var compressedBytes = _compressor.Compress(serializedBytes);
            return compressedBytes;
        }

        public async Task<NimbusMessage> DeserializeNimbusMessage(Message message)
        {
            byte[] compressedBytes;

            object blobId;
            string storageKey = null;
            var isLargeMessage = message.UserProperties.TryGetValue(MessagePropertyKeys.LargeBodyBlobIdentifier, out blobId);
            if (isLargeMessage)
            {
                storageKey = (string) blobId;
                compressedBytes = await _largeMessageBodyStore.Retrieve(storageKey);
            }
            else
            {
                compressedBytes = message.GetBody<byte[]>();
            }

            var serializedBytes = _compressor.Decompress(compressedBytes);
            var serializedString = Encoding.UTF8.GetString(serializedBytes);
            var deserialized = _serializer.Deserialize(serializedString, typeof (NimbusMessage));
            var nimbusMessage = (NimbusMessage) deserialized;

            if (!(nimbusMessage.Payload is IBusEvent))
            {
                if (isLargeMessage)
                {
                    await _largeMessageBodyStore.Delete(storageKey);
                }
            }

            return nimbusMessage;
        }
    }
}