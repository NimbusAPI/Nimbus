using System.Text;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal abstract class NatsJetStreamMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly NatsJetStreamContextFactory _jsContextFactory;
        private readonly ISerializer _serializer;
        protected readonly ILogger _logger;
        private INatsJSConsumer? _consumer;

        protected abstract string StreamName { get; }
        protected abstract string Subject { get; }
        protected abstract string ConsumerName { get; }
        protected abstract StreamConfigRetention StreamRetention { get; }

        protected NatsJetStreamMessageReceiver(
            NatsJetStreamContextFactory jsContextFactory,
            ISerializer serializer,
            ConcurrentHandlerLimitSetting concurrentHandlerLimit,
            IGlobalHandlerThrottle globalHandlerThrottle,
            ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _jsContextFactory = jsContextFactory;
            _serializer = serializer;
            _logger = logger;
        }

        protected override async Task WarmUp()
        {
            await _jsContextFactory.EnsureStreamAsync(StreamName, Subject, StreamRetention);
            _consumer = await _jsContextFactory.EnsureConsumerAsync(StreamName, new ConsumerConfig
            {
                Name = ConsumerName,
                DurableName = ConsumerName,
                FilterSubject = Subject,
                AckPolicy = ConsumerConfigAckPolicy.Explicit,
                DeliverPolicy = ConsumerConfigDeliverPolicy.All,
            });
        }

        protected override async Task<NimbusMessage?> Fetch(CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            try
            {
                await foreach (var msg in _consumer!.FetchAsync<byte[]>(
                    new NatsJSFetchOpts { MaxMsgs = 1, Expires = TimeSpan.FromSeconds(9) },
                    cancellationToken: cts.Token))
                {
                    await msg.AckAsync();
                    if (msg.Data == null) continue;
                    var nimbusMessage = (NimbusMessage)_serializer.Deserialize(
                        Encoding.UTF8.GetString(msg.Data), typeof(NimbusMessage));
                    return OnMessageReceived(nimbusMessage);
                }
                return null;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return null;
            }
        }

        // Override to decorate the received message before returning it to the pump.
        protected virtual NimbusMessage OnMessageReceived(NimbusMessage message) => message;

        // Sanitise an arbitrary Nimbus path into a name safe for NATS stream / consumer names.
        protected static string SanitiseName(string path)
        {
            var safe = System.Text.RegularExpressions.Regex.Replace(path, @"[^a-zA-Z0-9_-]", "_");
            return safe.Length > 240 ? safe[..240] : safe;
        }
    }
}
