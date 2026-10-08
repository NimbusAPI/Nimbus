using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Serializers.Json;
using Nimbus.Transports.SqlServer;
using Nimbus.Transports.SqlServer.MessageSendersAndReceivers;
using Nimbus.Transports.SqlServer.QueueManagement;
using Nimbus.Transports.SqlServer.Schema;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.SqlServerTransportTests
{
    /// <summary>
    ///     White-box tests against a live SQL Server. Set NIMBUS_TEST_SQLSERVER to a connection string to run them.
    ///     Staleness is simulated by back-dating LastSeenAt rather than waiting out the 5 minute minimum TTL.
    /// </summary>
    [TestFixture]
    public class WhenAnIdleSubscriptionExpires
    {
        private SqlServerTransportConfiguration _configuration;
        private SqlServerSubscription _subscription;
        private AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private NullLogger _logger;
        private JsonSerializer _serializer;

        [SetUp]
        public async Task SetUp()
        {
            var connectionString = Environment.GetEnvironmentVariable("NIMBUS_TEST_SQLSERVER");
            if (string.IsNullOrWhiteSpace(connectionString)) Assert.Ignore("Set NIMBUS_TEST_SQLSERVER to run SQL Server tests.");

            _logger = new NullLogger();
            _configuration = new SqlServerTransportConfiguration().WithConnectionString(connectionString).WithAutoCreateSchema();
            await new SqlServerSchemaCreator(_configuration, _logger).EnsureSchemaExists();

            _subscription = new SqlServerSubscription($"t.Idle.{Guid.NewGuid():N}", $"sub.{Guid.NewGuid():N}");
            _autoDeleteOnIdle = new AutoDeleteOnIdleSetting {Value = TimeSpan.FromMinutes(5)};
            _serializer = new JsonSerializer();
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_subscription == null) return;
            await Execute("DELETE FROM NimbusSubscriptions WHERE TopicName = @t; DELETE FROM NimbusMessages WHERE Destination = @q",
                          ("@t", _subscription.TopicPath), ("@q", _subscription.SubscriberQueueName));
        }

        [Test]
        public async Task WarmUpRegistersTheSubscriptionWithAFreshHeartbeat()
        {
            var receiver = CreateReceiver();
            await receiver.Start(msg => Task.CompletedTask);
            try
            {
                (await SubscriptionAgeSeconds()).ShouldNotBeNull().ShouldBeLessThan(30);
            }
            finally
            {
                receiver.Dispose();
            }
        }

        [Test]
        public async Task ReRegisteringARegisteredSubscriptionRefreshesItsHeartbeat()
        {
            await RegisterSubscription();
            await BackdateSubscription(TimeSpan.FromHours(1));

            await RegisterSubscription();

            (await SubscriptionAgeSeconds()).ShouldNotBeNull().ShouldBeLessThan(30);
        }

        [Test]
        public async Task TheSenderSkipsSubscriptionsWithALapsedHeartbeat()
        {
            await RegisterSubscription();
            var sender = new SqlServerTopicSender(_subscription.TopicPath, _configuration, _serializer, _autoDeleteOnIdle);

            await sender.Send(new NimbusMessage(_subscription.TopicPath));
            (await QueuedMessages()).ShouldBe(1);

            await BackdateSubscription(TimeSpan.FromHours(1));
            await sender.Send(new NimbusMessage(_subscription.TopicPath));
            (await QueuedMessages()).ShouldBe(1);
        }

        [Test]
        public async Task TheReaperRemovesStaleSubscriptionsAndTheirMessagesButKeepsLiveOnes()
        {
            var live = new SqlServerSubscription(_subscription.TopicPath, $"live.{Guid.NewGuid():N}");
            await RegisterSubscription();
            await RegisterSubscription(live);
            var sender = new SqlServerTopicSender(_subscription.TopicPath, _configuration, _serializer, _autoDeleteOnIdle);
            await sender.Send(new NimbusMessage(_subscription.TopicPath));
            await BackdateSubscription(TimeSpan.FromHours(1));

            await new SqlServerIdleSubscriptionReaper(_configuration, _autoDeleteOnIdle, _logger).ReapOnce();

            (await SubscriptionAgeSeconds()).ShouldBeNull();
            (await QueuedMessages()).ShouldBe(0);
            (await SubscriptionAgeSeconds(live)).ShouldNotBeNull();
            (await QueuedMessages(live)).ShouldBe(1);

            await Execute("DELETE FROM NimbusSubscriptions WHERE SubscriberQueue = @q; DELETE FROM NimbusMessages WHERE Destination = @q",
                          ("@q", live.SubscriberQueueName));
        }

        private SqlServerTopicReceiver CreateReceiver()
        {
            return new SqlServerTopicReceiver(_subscription,
                                             _configuration,
                                             _autoDeleteOnIdle,
                                             _serializer,
                                             new ConcurrentHandlerLimitSetting(),
                                             new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting()),
                                             _logger);
        }

        private Task RegisterSubscription(SqlServerSubscription subscription = null)
        {
            subscription ??= _subscription;
            return Execute(@"MERGE NimbusSubscriptions WITH (HOLDLOCK) AS target
                             USING (SELECT @t AS TopicName, @q AS SubscriberQueue) AS source
                                ON target.TopicName = source.TopicName AND target.SubscriberQueue = source.SubscriberQueue
                             WHEN MATCHED THEN UPDATE SET LastSeenAt = SYSUTCDATETIME()
                             WHEN NOT MATCHED THEN INSERT (TopicName, SubscriberQueue, LastSeenAt) VALUES (source.TopicName, source.SubscriberQueue, SYSUTCDATETIME());",
                           ("@t", subscription.TopicPath), ("@q", subscription.SubscriberQueueName));
        }

        private Task BackdateSubscription(TimeSpan age)
        {
            return Execute("UPDATE NimbusSubscriptions SET LastSeenAt = DATEADD(SECOND, -@s, SYSUTCDATETIME()) WHERE SubscriberQueue = @q",
                           ("@s", (int) age.TotalSeconds), ("@q", _subscription.SubscriberQueueName));
        }

        private async Task<double?> SubscriptionAgeSeconds(SqlServerSubscription subscription = null)
        {
            subscription ??= _subscription;
            var result = await Scalar("SELECT DATEDIFF(SECOND, LastSeenAt, SYSUTCDATETIME()) FROM NimbusSubscriptions WHERE SubscriberQueue = @q",
                                      ("@q", subscription.SubscriberQueueName));
            return result == null ? (double?) null : Convert.ToDouble(result);
        }

        private async Task<long> QueuedMessages(SqlServerSubscription subscription = null)
        {
            subscription ??= _subscription;
            return Convert.ToInt64(await Scalar("SELECT COUNT(*) FROM NimbusMessages WHERE Destination = @q", ("@q", subscription.SubscriberQueueName)));
        }

        private async Task Execute(string sql, params (string Name, object Value)[] parameters)
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
            await command.ExecuteNonQueryAsync();
        }

        private async Task<object> Scalar(string sql, params (string Name, object Value)[] parameters)
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
            var result = await command.ExecuteScalarAsync();
            return result is DBNull ? null : result;
        }
    }
}
