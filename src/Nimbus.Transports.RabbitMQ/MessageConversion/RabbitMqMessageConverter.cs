using System;
using System.Text;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using RabbitMQ.Client;

namespace Nimbus.Transports.RabbitMQ.MessageConversion
{
    internal class RabbitMqMessageConverter
    {
        private readonly ISerializer _serializer;
        private readonly ICompressor _compressor;
        private readonly ILogger _logger;

        public RabbitMqMessageConverter(ISerializer serializer, ICompressor compressor, ILogger logger)
        {
            _serializer = serializer;
            _compressor = compressor;
            _logger = logger;
        }

        public (byte[] body, BasicProperties properties) ToRabbitMq(NimbusMessage nimbusMessage, IClock clock)
        {
            var serialized = _serializer.Serialize(nimbusMessage);
            var body = _compressor.Compress(Encoding.UTF8.GetBytes(serialized));

            var props = new BasicProperties
            {
                MessageId = nimbusMessage.MessageId.ToString(),
                CorrelationId = nimbusMessage.CorrelationId.ToString(),
                Persistent = true,
                ContentType = "application/octet-stream",
            };

            var ttl = nimbusMessage.ExpiresAfter.Subtract(clock.UtcNow);
            if (nimbusMessage.ExpiresAfter < DateTimeOffset.MaxValue && ttl > TimeSpan.Zero)
                props.Expiration = ((long)ttl.TotalMilliseconds).ToString();

            var delay = nimbusMessage.DeliverAfter.Subtract(clock.UtcNow);
            if (delay > TimeSpan.Zero)
            {
                props.Headers ??= new System.Collections.Generic.Dictionary<string, object>();
                props.Headers["x-delay"] = (int)Math.Min(delay.TotalMilliseconds, int.MaxValue);
            }

            _logger.Debug("Encoded NimbusMessage {MessageId}", nimbusMessage.MessageId);
            return (body, props);
        }

        public NimbusMessage FromRabbitMq(ReadOnlyMemory<byte> body)
        {
            var decompressed = _compressor.Decompress(body.ToArray());
            var serialized = Encoding.UTF8.GetString(decompressed);
            var message = (NimbusMessage)_serializer.Deserialize(serialized, typeof(NimbusMessage));
            _logger.Debug("Decoded NimbusMessage {MessageId}", message.MessageId);
            return message;
        }
    }
}
