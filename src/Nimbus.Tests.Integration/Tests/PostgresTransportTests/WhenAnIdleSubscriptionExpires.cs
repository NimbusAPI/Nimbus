using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Serializers.Json;
using Nimbus.Transports.Postgres;
using Nimbus.Transports.Postgres.MessageSendersAndReceivers;
using Nimbus.Transports.Postgres.QueueManagement;
using Nimbus.Transports.Postgres.Schema;
using Npgsql;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.PostgresTransportTests
{
    /// <summary>
    ///     White-box tests against a live PostgreSQL. Set NIMBUS_TEST_POSTGRES to a connection string to run them.
    ///     Staleness is simulated by back-dating last_seen_at rather than waiting out the 5 minute minimum TTL.
    /// </summary>
    [TestFixture]
    public class WhenAnIdleSubscriptionExpires
    {
        private PostgresTransportConfiguration _configuration;
        private PostgresSubscription _subscription;
        private AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private NullLogger _logger;
        private JsonSerializer _serializer;

        [SetUp]
        public async Task SetUp()
        {
            var connectionString = Environment.GetEnvironmentVariable("NIMBUS_TEST_POSTGRES");
            if (string.IsNullOrWhiteSpace(connectionString)) Assert.Ignore("Set NIMBUS_TEST_POSTGRES to run PostgreSQL tests.");

            _logger = new NullLogger();
            _configuration = new PostgresTransportConfiguration().WithConnectionString(connectionString).WithAutoCreateSchema();
            await new PostgresSchemaCreator(_configuration, _logger).EnsureSchemaExists();

            _subscription = new PostgresSubscription($"t.Idle.{Guid.NewGuid():N}", $"sub.{Guid.NewGuid():N}");
            _autoDeleteOnIdle = new AutoDeleteOnIdleSetting {Value = TimeSpan.FromMinutes(5)};
            _serializer = new JsonSerializer();
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_subscription == null) return;
            await Execute("DELETE FROM nimbus_subscriptions WHERE topic_name = @t; DELETE FROM nimbus_messages WHERE destination = @q",
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
            var sender = new PostgresTopicSender(_subscription.TopicPath, _configuration, _serializer, _autoDeleteOnIdle);

            await sender.Send(new NimbusMessage(_subscription.TopicPath));
            (await QueuedMessages()).ShouldBe(1);

            await BackdateSubscription(TimeSpan.FromHours(1));
            await sender.Send(new NimbusMessage(_subscription.TopicPath));
            (await QueuedMessages()).ShouldBe(1);
        }

        [Test]
        public async Task TheReaperRemovesStaleSubscriptionsAndTheirMessagesButKeepsLiveOnes()
        {
            var live = new PostgresSubscription(_subscription.TopicPath, $"live.{Guid.NewGuid():N}");
            await RegisterSubscription();
            await RegisterSubscription(live);
            var sender = new PostgresTopicSender(_subscription.TopicPath, _configuration, _serializer, _autoDeleteOnIdle);
            await sender.Send(new NimbusMessage(_subscription.TopicPath));
            await BackdateSubscription(TimeSpan.FromHours(1));

            await new PostgresIdleSubscriptionReaper(_configuration, _autoDeleteOnIdle, _logger).ReapOnce();

            (await SubscriptionAgeSeconds()).ShouldBeNull();
            (await QueuedMessages()).ShouldBe(0);
            (await SubscriptionAgeSeconds(live)).ShouldNotBeNull();
            (await QueuedMessages(live)).ShouldBe(1);

            await Execute("DELETE FROM nimbus_subscriptions WHERE subscriber_queue = @q; DELETE FROM nimbus_messages WHERE destination = @q",
                          ("@q", live.SubscriberQueueName));
        }

        private PostgresTopicReceiver CreateReceiver()
        {
            return new PostgresTopicReceiver(_subscription,
                                             _configuration,
                                             _autoDeleteOnIdle,
                                             _serializer,
                                             new ConcurrentHandlerLimitSetting(),
                                             new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting()),
                                             _logger);
        }

        private Task RegisterSubscription(PostgresSubscription subscription = null)
        {
            subscription ??= _subscription;
            return Execute(@"INSERT INTO nimbus_subscriptions (topic_name, subscriber_queue, last_seen_at) VALUES (@t, @q, now())
                             ON CONFLICT (topic_name, subscriber_queue) DO UPDATE SET last_seen_at = now()",
                           ("@t", subscription.TopicPath), ("@q", subscription.SubscriberQueueName));
        }

        private Task BackdateSubscription(TimeSpan age)
        {
            return Execute("UPDATE nimbus_subscriptions SET last_seen_at = now() - make_interval(secs => @s) WHERE subscriber_queue = @q",
                           ("@s", age.TotalSeconds), ("@q", _subscription.SubscriberQueueName));
        }

        private async Task<double?> SubscriptionAgeSeconds(PostgresSubscription subscription = null)
        {
            subscription ??= _subscription;
            var result = await Scalar("SELECT extract(epoch FROM now() - last_seen_at) FROM nimbus_subscriptions WHERE subscriber_queue = @q",
                                      ("@q", subscription.SubscriberQueueName));
            return result == null ? (double?) null : Convert.ToDouble(result);
        }

        private async Task<long> QueuedMessages(PostgresSubscription subscription = null)
        {
            subscription ??= _subscription;
            return Convert.ToInt64(await Scalar("SELECT count(*) FROM nimbus_messages WHERE destination = @q", ("@q", subscription.SubscriberQueueName)));
        }

        private async Task Execute(string sql, params (string Name, object Value)[] parameters)
        {
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(sql, connection);
            foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
            await command.ExecuteNonQueryAsync();
        }

        private async Task<object> Scalar(string sql, params (string Name, object Value)[] parameters)
        {
            using var connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(sql, connection);
            foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
            var result = await command.ExecuteScalarAsync();
            return result is DBNull ? null : result;
        }
    }
}
