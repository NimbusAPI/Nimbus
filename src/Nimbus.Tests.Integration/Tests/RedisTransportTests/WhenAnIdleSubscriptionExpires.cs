using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Retries;
using Nimbus.InfrastructureContracts;
using Nimbus.Serializers.Json;
using Nimbus.Tests.Integration.Configuration;
using Nimbus.Transports.Redis.MessageSendersAndReceivers;
using Nimbus.Transports.Redis.QueueManagement;
using NUnit.Framework;
using Shouldly;
using StackExchange.Redis;

namespace Nimbus.Tests.Integration.Tests.RedisTransportTests
{
    [TestFixture]
    public class WhenAnIdleSubscriptionExpires
    {
        private ConnectionMultiplexer _multiplexer;
        private IDatabase _database;
        private Subscription _subscription;
        private AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private NullLogger _logger;
        private IRetry _retry;
        private JsonSerializer _serializer;

        [SetUp]
        public void SetUp()
        {
            var connectionString = AppSettingsLoader.Settings.Transports.Redis.ConnectionString;
            _multiplexer = ConnectionMultiplexer.Connect(connectionString);
            _database = _multiplexer.GetDatabase();

            var topicPath = $"t.IdleSubscriptionTests.{Guid.NewGuid():N}";
            var subscriptionName = $"sub.{Guid.NewGuid():N}";
            _subscription = new Subscription(topicPath, subscriptionName);

            _autoDeleteOnIdle = new AutoDeleteOnIdleSetting {Value = TimeSpan.FromSeconds(5)};
            _logger = new NullLogger();
            _retry = new DefaultRetry(_logger);
            _serializer = new JsonSerializer();
        }

        [TearDown]
        public void TearDown()
        {
            _database.KeyDelete(_subscription.TopicSubscribersRedisKey);
            _database.KeyDelete(_subscription.SubscriptionMessagesRedisKey);
            _database.KeyDelete(_subscription.SubscriberAliveRedisKey);
            _multiplexer.Dispose();
        }

        private RedisSubscriptionReceiver CreateReceiver()
        {
            return new RedisSubscriptionReceiver(
                _subscription,
                () => _multiplexer,
                () => _database,
                _serializer,
                new ConcurrentHandlerLimitSetting(),
                new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting()),
                _logger,
                _retry,
                _autoDeleteOnIdle);
        }

        [Test]
        public async Task WarmUpRegistersTheSubscriberAndSetsALivenessKeyWithTtl()
        {
            var receiver = CreateReceiver();

            await receiver.Start(msg => Task.CompletedTask);
            try
            {
                _database.SetContains(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey).ShouldBeTrue();

                var ttl = _database.KeyTimeToLive(_subscription.SubscriberAliveRedisKey);
                ttl.HasValue.ShouldBeTrue();
                ttl.Value.ShouldBeLessThanOrEqualTo(_autoDeleteOnIdle.Value);
            }
            finally
            {
                await receiver.Stop();
                receiver.Dispose();
            }
        }

        [Test]
        public async Task SendPrunesADeadSubscriberInsteadOfPushingToIt()
        {
            // No liveness key is set up - simulates a subscriber whose TTL has already expired.
            _database.SetAdd(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey);

            var sender = new RedisTopicSender(_subscription.TopicPath, _serializer, () => _database);
            await sender.Send(new NimbusMessage(_subscription.TopicPath));

            _database.SetContains(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey).ShouldBeFalse();
            _database.ListLength(_subscription.SubscriptionMessagesRedisKey).ShouldBe(0);
        }

        [Test]
        public async Task SendStillDeliversToALiveSubscriber()
        {
            _database.SetAdd(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey);
            _database.StringSet(_subscription.SubscriberAliveRedisKey, true, _autoDeleteOnIdle.Value);

            var sender = new RedisTopicSender(_subscription.TopicPath, _serializer, () => _database);
            await sender.Send(new NimbusMessage(_subscription.TopicPath));

            _database.SetContains(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey).ShouldBeTrue();
            _database.ListLength(_subscription.SubscriptionMessagesRedisKey).ShouldBe(1);
        }

        [Test]
        public async Task ReaperRemovesAColdSubscriptionThatNobodyIsPublishingTo()
        {
            // Nothing publishes to this topic, so RedisTopicSender never gets a chance to prune it -
            // only the reaper's own sweep can clean it up.
            _database.SetAdd(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey);
            _database.ListRightPush(_subscription.SubscriptionMessagesRedisKey, "orphaned-message");

            var reaper = new RedisIdleSubscriptionReaper(() => _multiplexer, _autoDeleteOnIdle, _logger);
            await reaper.ReapOnce();

            _database.SetContains(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey).ShouldBeFalse();
            _database.KeyExists(_subscription.SubscriptionMessagesRedisKey).ShouldBeFalse();
        }

        [Test]
        public async Task ReaperLeavesALiveSubscriptionAlone()
        {
            _database.SetAdd(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey);
            _database.StringSet(_subscription.SubscriberAliveRedisKey, true, _autoDeleteOnIdle.Value);

            var reaper = new RedisIdleSubscriptionReaper(() => _multiplexer, _autoDeleteOnIdle, _logger);
            await reaper.ReapOnce();

            _database.SetContains(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey).ShouldBeTrue();
        }
    }
}
