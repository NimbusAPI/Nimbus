using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Compression;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Serializers.Json;
using Nimbus.Transports.RabbitMQ;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.RabbitMqTransportTests
{
    /// <summary>
    ///     White-box tests against a live RabbitMQ (with the delayed message exchange plugin) or LavinMQ.
    ///     Set NIMBUS_TEST_RABBITMQ to "host:port" (e.g. localhost:5674) to run them. The broker's default
    ///     user is "guest"/"guest"; set NIMBUS_TEST_RABBITMQ_CREDENTIALS to "user:password" to override.
    /// </summary>
    [TestFixture]
    public class WhenAnIdleSubscriptionExpires
    {
        private RabbitMqTransportConfiguration _configuration;
        private RabbitMqConnectionManager _connectionManager;
        private RabbitMqSubscription _subscription;
        private NullLogger _logger;

        [SetUp]
        public void SetUp()
        {
            var endpoint = Environment.GetEnvironmentVariable("NIMBUS_TEST_RABBITMQ");
            if (string.IsNullOrWhiteSpace(endpoint)) Assert.Ignore("Set NIMBUS_TEST_RABBITMQ to run RabbitMQ tests.");

            var parts = endpoint.Split(':');
            var credentials = (Environment.GetEnvironmentVariable("NIMBUS_TEST_RABBITMQ_CREDENTIALS") ?? "guest:guest").Split(':');
            _logger = new NullLogger();
            _configuration = new RabbitMqTransportConfiguration().WithHost(parts[0]).WithPort(int.Parse(parts[1])).WithCredentials(credentials[0], credentials[1]);
            _connectionManager = new RabbitMqConnectionManager(_configuration, _logger);
            _subscription = new RabbitMqSubscription($"t.idle.{Guid.NewGuid():N}", $"sub.{Guid.NewGuid():N}");
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_connectionManager == null) return;

            var channel = await _connectionManager.CreateChannelAsync();
            try
            {
                await channel.QueueDeleteAsync(_subscription.SubscriptionName);
                await channel.ExchangeDeleteAsync(_subscription.TopicPath);
            }
            catch { }
            await _connectionManager.DisposeAsync();
        }

        [Test]
        public async Task AnAbandonedSubscriptionQueueIsDeletedByTheBroker()
        {
            var receiver = CreateReceiver(TimeSpan.FromSeconds(2));
            await receiver.Start(msg => Task.CompletedTask);
            (await QueueExists()).ShouldBeTrue();

            receiver.Dispose();

            // Don't poll: redeclaring a queue (even passively) counts as use and renews its expiry.
            await Task.Delay(TimeSpan.FromSeconds(10));
            (await QueueExists()).ShouldBeFalse();
        }

        [Test]
        public async Task ASubscriptionQueueWithALiveConsumerIsNotDeleted()
        {
            var receiver = CreateReceiver(TimeSpan.FromSeconds(2));
            await receiver.Start(msg => Task.CompletedTask);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(8));
                (await QueueExists()).ShouldBeTrue();
            }
            finally
            {
                receiver.Dispose();
            }
        }

        [Test]
        public async Task AQueueDeclaredBeforeIdleCleanupExistedIsStillUsable()
        {
            var channel = await _connectionManager.CreateChannelAsync();
            await channel.QueueDeclareAsync(_subscription.SubscriptionName, durable: true, exclusive: false, autoDelete: false);

            var receiver = CreateReceiver(TimeSpan.FromMinutes(5));
            await receiver.Start(msg => Task.CompletedTask);
            try
            {
                (await QueueExists()).ShouldBeTrue();
            }
            finally
            {
                receiver.Dispose();
            }
        }

        private RabbitMqTopicReceiver CreateReceiver(TimeSpan idle)
        {
            return new RabbitMqTopicReceiver(_subscription,
                                             null,
                                             _connectionManager,
                                             new RabbitMqMessageConverter(new JsonSerializer(), new NullCompressor(), _logger),
                                             new AutoDeleteOnIdleSetting {Value = idle},
                                             new ConcurrentHandlerLimitSetting(),
                                             new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting()),
                                             _logger);
        }

        private async Task<bool> QueueExists()
        {
            var channel = await _connectionManager.CreateChannelAsync();
            try
            {
                await channel.QueueDeclarePassiveAsync(_subscription.SubscriptionName);
                await channel.CloseAsync();
                return true;
            }
            catch (OperationInterruptedException)
            {
                return false;
            }
        }
    }
}
