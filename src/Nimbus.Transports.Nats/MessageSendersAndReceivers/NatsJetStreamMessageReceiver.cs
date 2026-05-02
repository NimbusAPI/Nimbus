using System.Text;
using System.Threading.Channels;
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
        protected readonly NatsJetStreamContextFactory _jsContextFactory;
        private readonly ISerializer _serializer;
        protected readonly ILogger _logger;

        private Channel<NimbusMessage>? _channel;
        private INatsJSConsumer? _mainConsumer;
        private readonly List<INatsJSConsumer> _additionalConsumers = new();
        private volatile bool _loopStarted;

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
            _loopStarted = false;
            _additionalConsumers.Clear();
            // SingleWriter = false: multiple consumer loops (main + retry) write concurrently.
            _channel = Channel.CreateUnbounded<NimbusMessage>(new UnboundedChannelOptions { SingleWriter = false });

            await _jsContextFactory.EnsureStreamAsync(StreamName, Subject, StreamRetention);
            _mainConsumer = await _jsContextFactory.EnsureConsumerAsync(StreamName, new ConsumerConfig
            {
                Name = ConsumerName,
                DurableName = ConsumerName,
                FilterSubject = Subject,
                AckPolicy = ConsumerConfigAckPolicy.Explicit,
                DeliverPolicy = ConsumerConfigDeliverPolicy.All,
            });

            await OnWarmingUp();

            // PingAsync gives a PING/PONG round-trip confirming the server has processed all
            // setup (stream, consumer) before WarmUp returns and the test starts publishing.
            await _jsContextFactory.PingAsync();
        }

        // Override to register additional consumers (e.g. a per-subscription retry consumer).
        // Use RegisterAdditionalConsumer() — loops are started lazily in Fetch() so they use
        // the base class's cancellation token and stop cleanly when Stop() is called.
        protected virtual Task OnWarmingUp() => Task.CompletedTask;

        protected void RegisterAdditionalConsumer(INatsJSConsumer consumer)
        {
            _additionalConsumers.Add(consumer);
        }

        protected override async Task<NimbusMessage?> Fetch(CancellationToken cancellationToken)
        {
            if (_channel == null) return null;

            // Start consumer loops on the first Fetch, using the base class's cancellation token.
            // This ties loop lifetime to Stop() — when Stop() cancels its token, loops cancel too.
            if (!_loopStarted)
            {
                _loopStarted = true;
                StartConsumerLoop(_mainConsumer!, cancellationToken);
                foreach (var consumer in _additionalConsumers)
                    StartConsumerLoop(consumer, cancellationToken);
            }

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

        private void StartConsumerLoop(INatsJSConsumer consumer, CancellationToken ct)
        {
            var channel = _channel!;
            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await foreach (var msg in consumer.ConsumeAsync<byte[]>(cancellationToken: ct))
                        {
                            if (msg.Data == null) { await msg.AckAsync(cancellationToken: ct); continue; }
                            var nimbusMessage = (NimbusMessage)_serializer.Deserialize(
                                Encoding.UTF8.GetString(msg.Data), typeof(NimbusMessage));
                            await channel.Writer.WriteAsync(OnMessageReceived(nimbusMessage), CancellationToken.None);
                            await msg.AckAsync(cancellationToken: ct);
                        }
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception) when (ct.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "JetStream consume loop failed for {Consumer}", ConsumerName);
                        await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
                    }
                }
            }, CancellationToken.None);
        }

        protected virtual NimbusMessage OnMessageReceived(NimbusMessage message) => message;

        protected static string SanitiseName(string path) => NatsNameSanitiser.Sanitise(path);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _channel?.Writer.TryComplete();
            }
            base.Dispose(disposing);
        }
    }
}
