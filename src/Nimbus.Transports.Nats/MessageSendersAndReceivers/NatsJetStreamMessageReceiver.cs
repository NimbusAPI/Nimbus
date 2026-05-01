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
        private CancellationTokenSource? _cts;

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
            _cts = new CancellationTokenSource();
            // SingleWriter = false: subclasses may add additional consumer loops writing concurrently.
            _channel = Channel.CreateUnbounded<NimbusMessage>(new UnboundedChannelOptions { SingleWriter = false });

            await _jsContextFactory.EnsureStreamAsync(StreamName, Subject, StreamRetention);
            var consumer = await _jsContextFactory.EnsureConsumerAsync(StreamName, new ConsumerConfig
            {
                Name = ConsumerName,
                DurableName = ConsumerName,
                FilterSubject = Subject,
                AckPolicy = ConsumerConfigAckPolicy.Explicit,
                DeliverPolicy = ConsumerConfigDeliverPolicy.All,
            });

            StartConsumerLoop(consumer);
            await OnWarmingUp();

            // PingAsync gives a PING/PONG round-trip confirming the server has processed all
            // setup (stream, consumer) before WarmUp returns and the test starts publishing.
            await _jsContextFactory.PingAsync();
        }

        // Override to start additional consumer loops (e.g. a per-subscription retry consumer).
        protected virtual Task OnWarmingUp() => Task.CompletedTask;

        protected void StartConsumerLoop(INatsJSConsumer consumer)
        {
            var ct = _cts!.Token;
            var channel = _channel!;
            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await foreach (var msg in consumer.ConsumeAsync<byte[]>(cancellationToken: ct))
                        {
                            await msg.AckAsync(cancellationToken: ct);
                            if (msg.Data == null) continue;
                            var nimbusMessage = (NimbusMessage)_serializer.Deserialize(
                                Encoding.UTF8.GetString(msg.Data), typeof(NimbusMessage));
                            await channel.Writer.WriteAsync(OnMessageReceived(nimbusMessage), CancellationToken.None);
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
                        await Task.Delay(TimeSpan.FromSeconds(3), CancellationToken.None);
                    }
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

        protected virtual NimbusMessage OnMessageReceived(NimbusMessage message) => message;

        protected static string SanitiseName(string path)
        {
            var safe = System.Text.RegularExpressions.Regex.Replace(path, @"[^a-zA-Z0-9_-]", "_");
            return safe.Length > 240 ? safe[..240] : safe;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _channel?.Writer.TryComplete();
            }
            base.Dispose(disposing);
        }
    }
}
