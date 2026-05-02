using System.Text;
using System.Threading.Channels;
using NATS.Client.Core;
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
            // SingleWriter = false because subclasses may add additional NATS subscriptions
            // that all write to the same channel concurrently.
            _channel = Channel.CreateUnbounded<NimbusMessage>(new UnboundedChannelOptions { SingleWriter = false });
            var ct = _subscriptionCts.Token;

            var connection = _connectionFactory.GetConnection();
            await connection.ConnectAsync();

            await AddNatsSubscription(connection, Subject, QueueGroup, ct);
            await OnWarmingUp(connection, ct);

            // PingAsync gives a PING/PONG round-trip, confirming the server has processed the SUB
            // before WarmUp returns and the test starts publishing.
            await connection.PingAsync(ct);
        }

        // Override to subscribe to additional NATS subjects (e.g. a per-subscription retry subject).
        protected virtual Task OnWarmingUp(NatsConnection connection, CancellationToken ct) => Task.CompletedTask;

        protected async Task AddNatsSubscription(NatsConnection connection, string subject, string queueGroup, CancellationToken ct)
        {
            var sub = await connection.SubscribeCoreAsync<byte[]>(subject, queueGroup: queueGroup, cancellationToken: ct);
            var channel = _channel!;
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
                    _logger.Error(ex, "NATS subscription loop failed for {Subject}/{QueueGroup}", subject, queueGroup);
                }
                finally
                {
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
