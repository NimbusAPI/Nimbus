using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AMQP.MessageConversion
{
    internal class NmsMessageFactory : INmsMessageFactory
    {
        private readonly ISerializer _serializer;
        private readonly ICompressor _compressor;
        private readonly IClock _clock;
        private readonly ILogger _logger;

        public NmsMessageFactory(ISerializer serializer, ICompressor compressor, IClock clock, ILogger logger)
        {
            _serializer = serializer;
            _compressor = compressor;
            _clock = clock;
            _logger = logger;
        }

        public Task<IBytesMessage> CreateNmsMessage(NimbusMessage nimbusMessage, ISession session)
        {
            return Task.Run(() =>
            {
                // Serialize and compress the NimbusMessage
                var serialized = _serializer.Serialize(nimbusMessage);
                var serializedBytes = Encoding.UTF8.GetBytes(serialized);
                var compressedBytes = _compressor.Compress(serializedBytes);

                // Create NMS bytes message
                var nmsMessage = session.CreateBytesMessage(compressedBytes);

                // Map core properties
                nmsMessage.NMSMessageId = nimbusMessage.MessageId.ToString();
                nmsMessage.NMSCorrelationID = nimbusMessage.CorrelationId.ToString();

                // Map custom properties
                foreach (var property in nimbusMessage.Properties)
                {
                    SetMessageProperty(nmsMessage, property.Key, property.Value);
                }

                // Set Time-To-Live
                var ttl = nimbusMessage.ExpiresAfter.Subtract(_clock.UtcNow);
                if (nimbusMessage.ExpiresAfter < DateTimeOffset.MaxValue && ttl > TimeSpan.Zero)
                {
                    nmsMessage.NMSTimeToLive = ttl;
                }

                // Set delayed delivery using Artemis scheduled delivery
                var delay = nimbusMessage.DeliverAfter.Subtract(_clock.UtcNow);
                if (delay > TimeSpan.Zero)
                {
                    nmsMessage.Properties["_AMQ_SCHED_DELIVERY"] = nimbusMessage.DeliverAfter.ToUnixTimeMilliseconds();
                }

                // Store delivery attempts count
                if (nimbusMessage.DeliveryAttempts != null && nimbusMessage.DeliveryAttempts.Any())
                {
                    nmsMessage.Properties["Nimbus.DeliveryAttempts"] = nimbusMessage.DeliveryAttempts.Length;
                }

                _logger.Debug("Created NMS message {MessageId} for queue/topic delivery", nimbusMessage.MessageId);

                return nmsMessage;
            });
        }

        public Task<NimbusMessage> CreateNimbusMessage(IMessage nmsMessage)
        {
            return Task.Run(() =>
            {
                if (!(nmsMessage is IBytesMessage bytesMessage))
                {
                    throw new InvalidOperationException($"Expected IBytesMessage but received {nmsMessage.GetType().Name}");
                }

                // Read the message body
                var messageBody = new byte[bytesMessage.BodyLength];
                bytesMessage.ReadBytes(messageBody);

                // Decompress and deserialize
                var decompressedBytes = _compressor.Decompress(messageBody);
                var serialized = Encoding.UTF8.GetString(decompressedBytes);
                var nimbusMessage = (NimbusMessage)_serializer.Deserialize(serialized, typeof(NimbusMessage));

                _logger.Debug("Created Nimbus message {MessageId} from NMS message", nimbusMessage.MessageId);

                return nimbusMessage;
            });
        }

        private void SetMessageProperty(IMessage message, string key, object value)
        {
            if (value == null) return;

            try
            {
                // NMS supports primitive types in properties
                switch (value)
                {
                    case string s:
                        message.Properties[key] = s;
                        break;
                    case int i:
                        message.Properties[key] = i;
                        break;
                    case long l:
                        message.Properties[key] = l;
                        break;
                    case bool b:
                        message.Properties[key] = b;
                        break;
                    case double d:
                        message.Properties[key] = d;
                        break;
                    case float f:
                        message.Properties[key] = f;
                        break;
                    case byte by:
                        message.Properties[key] = by;
                        break;
                    case short sh:
                        message.Properties[key] = sh;
                        break;
                    default:
                        // For complex types, serialize to string
                        message.Properties[key] = value.ToString();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to set message property {Key} with value type {ValueType}", key, value.GetType().Name);
            }
        }
    }
}
