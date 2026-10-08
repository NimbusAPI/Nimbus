using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;
using Timer = System.Timers.Timer;

namespace Nimbus.Transports.Nats.QueueManagement
{
    /// <summary>
    ///     Every topic subscription gets its own retry stream (see <c>NatsJetStreamTopicReceiver</c>). The
    ///     server drops idle consumers via <c>InactiveThreshold</c>, but streams have no idle expiry, so this
    ///     reaper deletes retry streams that have no consumers left.
    /// </summary>
    internal class NatsJetStreamRetryStreamReaper : IDisposable
    {
        private const string RetrySubjectSuffix = ".retry";

        private readonly NatsJetStreamContextFactory _jsContextFactory;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly ILogger _logger;
        private readonly HashSet<string> _consumerlessLastSweep = new();
        private Timer? _timer;

        public NatsJetStreamRetryStreamReaper(NatsJetStreamContextFactory jsContextFactory, AutoDeleteOnIdleSetting autoDeleteOnIdle, ILogger logger)
        {
            _jsContextFactory = jsContextFactory;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            _logger = logger;
        }

        public void Start()
        {
            if (_timer != null) return;

            var interval = TimeSpan.FromTicks(_autoDeleteOnIdle.Value.Ticks / 4);

            _timer = new Timer(interval.TotalMilliseconds) { AutoReset = true };
            _timer.Elapsed += (_, _) => Task.Run(ReapOnce).ConfigureAwaitFalse();
            _timer.Start();
        }

        // internal so tests can trigger a sweep synchronously.
        internal async Task ReapOnce()
        {
            try
            {
                var consumerless = (await _jsContextFactory.ListStreamsAsync())
                                   .Where(s => s.Config.Name != null)
                                   .Where(s => s.Config.Subjects?.Any(subject => subject.EndsWith(RetrySubjectSuffix, StringComparison.Ordinal)) == true)
                                   .Where(s => s.State.ConsumerCount == 0)
                                   .Select(s => s.Config.Name!)
                                   .ToHashSet();

                // A stream is created a moment before its consumer, so only delete one that was
                // already consumer-less on the previous sweep too.
                foreach (var name in consumerless.Where(_consumerlessLastSweep.Contains))
                {
                    _logger.Info("Deleting orphaned retry stream {Stream}", name);
                    await _jsContextFactory.DeleteStreamAsync(name);
                }

                _consumerlessLastSweep.Clear();
                foreach (var name in consumerless) _consumerlessLastSweep.Add(name);
            }
            catch (Exception exc)
            {
                _logger.Warn(exc, "Retry stream reaper sweep failed: {Message}", exc.Message);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
