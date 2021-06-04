namespace Nimbus.Transports.AzureServiceBus2.BrokeredMessages
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using Nimbus.Configuration.LargeMessages.Settings;
    using Nimbus.Extensions;
    using Nimbus.Infrastructure;
    using Nimbus.Infrastructure.Dispatching;
    using Nimbus.InfrastructureContracts;
    using Nimbus.MessageContracts;
    using Nimbus.MessageContracts.Exceptions;

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
            this._serializer = serializer;
            this._maxLargeMessageSize = maxLargeMessageSize;
            this._maxSmallMessageSize = maxSmallMessageSize;
            this._compressor = compressor;
            this._dispatchContextManager = dispatchContextManager;
            this._largeMessageBodyStore = largeMessageBodyStore;
            this._typeProvider = typeProvider;
            this._clock = clock;
        }

        public Task<ServiceBusMessage> BuildMessage(NimbusMessage nimbusMessage)
        {
            return Task.Run(async () =>
                                  {
                                      ServiceBusMessage brokeredMessage;
                                      var messageBodyBytes = SerializeNimbusMessage(nimbusMessage);

                                      if (messageBodyBytes.Length > this._maxLargeMessageSize)
                                      {
                                          var errorMessage =
                                              "Message body size of {0} is larger than the permitted maximum of {1}. You need to change this in your bus configuration settings if you want to send messages this large."
                                                  .FormatWith(messageBodyBytes.Length, this._maxLargeMessageSize.Value);
                                          throw new BusException(errorMessage);
                                      }

                                      if (messageBodyBytes.Length > this._maxSmallMessageSize)
                                      {
                                          brokeredMessage = new ServiceBusMessage();
                                          var expiresAfter = nimbusMessage.ExpiresAfter;
                                          var blobIdentifier = await this._largeMessageBodyStore.Store(nimbusMessage.MessageId, messageBodyBytes, expiresAfter);
                                          brokeredMessage.ApplicationProperties[MessagePropertyKeys.LargeBodyBlobIdentifier] = blobIdentifier;
                                      }
                                      else
                                      {
                                          brokeredMessage = new ServiceBusMessage(messageBodyBytes);
                                      }

                                      var currentDispatchContext = this._dispatchContextManager.GetCurrentDispatchContext();
                                      brokeredMessage.MessageId = nimbusMessage.MessageId.ToString();
                                      brokeredMessage.CorrelationId = currentDispatchContext.CorrelationId.ToString();
                                      brokeredMessage.ReplyTo = nimbusMessage.From;
                                      brokeredMessage.TimeToLive = nimbusMessage.ExpiresAfter.Subtract(DateTimeOffset.UtcNow);
                                      brokeredMessage.ScheduledEnqueueTime = nimbusMessage.DeliverAfter.UtcDateTime;

                                      foreach (var property in nimbusMessage.Properties)
                                      {
                                          brokeredMessage.ApplicationProperties[property.Key] = property.Value;
                                      }

                                      return brokeredMessage;
                                  });
        }

        public async Task<NimbusMessage> BuildNimbusMessage(ServiceBusMessage message)
        {
            var nimbusMessage = await this.DeserializeNimbusMessage(message);
            return nimbusMessage;
        }

        private byte[] SerializeNimbusMessage(object serializableObject)
        {
            var serializedString = this._serializer.Serialize(serializableObject);
            var serializedBytes = Encoding.UTF8.GetBytes(serializedString);
            var compressedBytes = this._compressor.Compress(serializedBytes);
            return compressedBytes;
        }

        public async Task<NimbusMessage> DeserializeNimbusMessage(ServiceBusMessage message)
        {
            byte[] compressedBytes;

            object blobId;
            string storageKey = null;
            var isLargeMessage = message.ApplicationProperties.TryGetValue(MessagePropertyKeys.LargeBodyBlobIdentifier, out blobId);
            if (isLargeMessage)
            {
                storageKey = (string) blobId;
                compressedBytes = await this._largeMessageBodyStore.Retrieve(storageKey);
            }
            else
            {
                compressedBytes = message.Body.ToArray();
            }

            var serializedBytes = this._compressor.Decompress(compressedBytes);
            var serializedString = Encoding.UTF8.GetString(serializedBytes);
            var deserialized = this._serializer.Deserialize(serializedString, typeof (NimbusMessage));
            var nimbusMessage = (NimbusMessage) deserialized;

            if (!(nimbusMessage.Payload is IBusEvent))
            {
                if (isLargeMessage)
                {
                    await this._largeMessageBodyStore.Delete(storageKey);
                }
            }

            return nimbusMessage;
        }
    }
}