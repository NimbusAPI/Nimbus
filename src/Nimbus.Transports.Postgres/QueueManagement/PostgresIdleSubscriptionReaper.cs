using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Npgsql;
using NpgsqlTypes;
using Timer = System.Timers.Timer;

namespace Nimbus.Transports.Postgres.QueueManagement
{
    /// <summary>
    ///     Removes subscriptions whose receivers have stopped sending heartbeats (see
    ///     <c>PostgresTopicReceiver</c>) along with any messages still queued for them. Every bus instance
    ///     runs a sweep; the statement is idempotent so concurrent sweeps are harmless.
    /// </summary>
    internal class PostgresIdleSubscriptionReaper : IDisposable
    {
        private readonly PostgresTransportConfiguration _configuration;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly ILogger _logger;
        private Timer _timer;

        // Uses database time throughout so clock skew between application hosts doesn't matter.
        private const string ReapSql = @"
            WITH dead AS (
                DELETE FROM nimbus_subscriptions
                WHERE  last_seen_at < now() - @idle_timeout
                RETURNING subscriber_queue
            )
            DELETE FROM nimbus_messages
            WHERE  destination IN (SELECT subscriber_queue FROM dead)";

        public PostgresIdleSubscriptionReaper(PostgresTransportConfiguration configuration, AutoDeleteOnIdleSetting autoDeleteOnIdle, ILogger logger)
        {
            _configuration = configuration;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            _logger = logger;
        }

        public void Start()
        {
            if (_timer != null) return;

            var interval = TimeSpan.FromTicks(_autoDeleteOnIdle.Value.Ticks / 4);

            _timer = new Timer(interval.TotalMilliseconds) {AutoReset = true};
            _timer.Elapsed += (s, e) => Task.Run(ReapOnce).ConfigureAwaitFalse();
            _timer.Start();
        }

        // internal so tests can trigger a sweep synchronously.
        internal async Task ReapOnce()
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.ConnectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(ReapSql, connection);
                command.Parameters.Add("@idle_timeout", NpgsqlDbType.Interval).Value = _autoDeleteOnIdle.Value;

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception exc)
            {
                _logger.Warn(exc, "Idle subscription reaper sweep failed: {Message}", exc.Message);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
