using System.Text;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.Amqp.Messages
{
    internal class MessageFactory : IMessageFactory
    {
        private readonly ISerializer _serializer;
        private readonly ICompressor _compressor;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly ILogger _logger;

        public MessageFactory(
            ISerializer serializer,
            ICompressor compressor,
            IDispatchContextManager dispatchContextManager,
            ILogger logger)
        {
            _serializer = serializer;
            _compressor = compressor;
            _dispatchContextManager = dispatchContextManager;
            _logger = logger;
        }

        public Task<Message> BuildMessage(NimbusMessage nimbusMessage)
        {
            return Task.Run(() =>
                            {
                                var message = new Message();
                                var messageBodyBytes = SerializeNimbusMessage(nimbusMessage);
                                message.BodySection = new Data {Binary = messageBodyBytes};
                                message.Properties = new global::Amqp.Framing.Properties();

                                var currentDispatchContext = _dispatchContextManager.GetCurrentDispatchContext();
                                message.Properties.MessageId = nimbusMessage.MessageId.ToString();
                                message.Properties.CorrelationId = currentDispatchContext.CorrelationId.ToString();
                                message.Properties.ReplyTo = nimbusMessage.From;
                                message.Properties.AbsoluteExpiryTime = nimbusMessage.ExpiresAfter.UtcDateTime;
                                message.Properties.To = nimbusMessage.To;

                                message.ApplicationProperties = new ApplicationProperties();
                                foreach (var property in nimbusMessage.Properties)
                                {
                                    message.ApplicationProperties[property.Key] = property.Value;
                                }

                                return message;
                            });
        }

        public Task<NimbusMessage> BuildNimbusMessage(Message message)
        {
            return Task.Run(() =>
                            {
                                byte[] compressedBytes = null;

                                if (message.Body != null)
                                {
                                    compressedBytes = message.Body as byte[];
                                }

                                var serializedBytes = _compressor.Decompress(compressedBytes);
                                var serializedString = Encoding.UTF8.GetString(serializedBytes);
                                var deserialized = _serializer.Deserialize(serializedString, typeof(NimbusMessage));
                                var nimbusMessage = (NimbusMessage) deserialized;

                                return nimbusMessage;
                            });
        }

        private byte[] SerializeNimbusMessage(object serializableObject)
        {
            var serializedString = _serializer.Serialize(serializableObject);
            var serializedBytes = Encoding.UTF8.GetBytes(serializedString);
            var compressedBytes = _compressor.Compress(serializedBytes);
            return compressedBytes;
        }
    }
}