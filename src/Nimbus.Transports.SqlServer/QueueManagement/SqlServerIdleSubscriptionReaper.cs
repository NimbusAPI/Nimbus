using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Timer = System.Timers.Timer;

namespace Nimbus.Transports.SqlServer.QueueManagement
{
    /// <summary>
    ///     Removes subscriptions whose receivers have stopped sending heartbeats (see
    ///     <c>SqlServerTopicReceiver</c>) along with any messages still queued for them. Every bus instance
    ///     runs a sweep; the batch is idempotent so concurrent sweeps are harmless.
    /// </summary>
    internal class SqlServerIdleSubscriptionReaper : IDisposable
    {
        private readonly SqlServerTransportConfiguration _configuration;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly ILogger _logger;
        private Timer _timer;

        // Uses database time throughout so clock skew between application hosts doesn't matter.
        private const string ReapSql = @"
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;

            DECLARE @Dead TABLE (SubscriberQueue NVARCHAR(255) NOT NULL);

            DELETE FROM NimbusSubscriptions
            OUTPUT DELETED.SubscriberQueue INTO @Dead
            WHERE LastSeenAt < DATEADD(SECOND, -@IdleTimeoutSeconds, SYSUTCDATETIME());

            DELETE FROM NimbusMessages
            WHERE Destination IN (SELECT SubscriberQueue FROM @Dead);

            COMMIT TRANSACTION;";

        public SqlServerIdleSubscriptionReaper(SqlServerTransportConfiguration configuration, AutoDeleteOnIdleSetting autoDeleteOnIdle, ILogger logger)
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
                using var connection = new SqlConnection(_configuration.ConnectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(ReapSql, connection);
                command.Parameters.Add("@IdleTimeoutSeconds", SqlDbType.Int).Value = (int) _autoDeleteOnIdle.Value.TotalSeconds;

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
