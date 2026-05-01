using System.Text;
using System.Threading.Channels;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal abstract class NatsMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly NatsConnectionFactory _connectionFactory;
        private readonly ISerializer _serializer;
        protected readonly ILogger _logger;

        private Channel<NimbusMessage>? _channel;
        private CancellationTokenSource? _subscriptionCts;

        protected abstract string Subject { get; }
        protected abstract string QueueGroup { get; }

        protected NatsMessageReceiver(
            NatsConnectionFactory connectionFactory,
            ISerializer serializer,
            ConcurrentHandlerLimitSetting concurrentHandlerLimit,
            IGlobalHandlerThrottle globalHandlerThrottle,
            ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _connectionFactory = connectionFactory;
            _serializer = serializer;
            _logger = logger;
        }

        protected override async Task WarmUp()
        {
            _subscriptionCts = new CancellationTokenSource();
            _channel = Channel.CreateUnbounded<NimbusMessage>(new UnboundedChannelOptions { SingleWriter = true });
            var ct = _subscriptionCts.Token;

            var connection = _connectionFactory.GetConnection();
            await connection.ConnectAsync();

            // SubscribeCoreAsync sends the SUB command and completes once the subscription is registered.
            // We store it so the background loop can iterate Msgs and so Dispose can clean it up.
            var sub = await connection.SubscribeCoreAsync<byte[]>(Subject, queueGroup: QueueGroup, cancellationToken: ct);

            // PingAsync gives a PING/PONG round-trip, confirming the server has processed the SUB
            // before WarmUp returns and the test starts publishing.
            await connection.PingAsync(ct);

            var channel = _channel;
            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var msg in sub.Msgs.ReadAllAsync(ct))
                    {
                        if (msg.Data == null) continue;
                        var nimbusMessage = (NimbusMessage)_serializer.Deserialize(
                            Encoding.UTF8.GetString(msg.Data), typeof(NimbusMessage));
                        await channel.Writer.WriteAsync(nimbusMessage, CancellationToken.None);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.Error(ex, "NATS subscription loop failed for {Subject}/{QueueGroup}", Subject, QueueGroup);
                }
                finally
                {
                    channel.Writer.TryComplete();
                    await sub.DisposeAsync();
                }
            }, CancellationToken.None);
        }

        protected override async Task<NimbusMessage?> Fetch(CancellationToken cancellationToken)
        {
            if (_channel == null) return null;

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(10));
                return await _channel.Reader.ReadAsync(cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            catch (ChannelClosedException)
            {
                return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subscriptionCts?.Cancel();
                _subscriptionCts?.Dispose();
                _channel?.Writer.TryComplete();
            }
            base.Dispose(disposing);
        }
    }
}
