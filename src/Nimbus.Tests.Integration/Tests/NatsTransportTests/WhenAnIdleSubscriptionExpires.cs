using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.InfrastructureContracts;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Serializers.Json;
using Nimbus.Transports.Nats;
using Nimbus.Transports.Nats.ConnectionManagement;
using Nimbus.Transports.Nats.MessageSendersAndReceivers;
using Nimbus.Transports.Nats.QueueManagement;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Integration.Tests.NatsTransportTests
{
    /// <summary>
    ///     White-box tests against a live NATS server with JetStream. Set NIMBUS_TEST_NATS to a URL
    ///     (e.g. nats://localhost:4222) to run them.
    /// </summary>
    [TestFixture]
    public class WhenAnIdleSubscriptionExpires
    {
        private NatsConnectionFactory _connectionFactory;
        private NatsJetStreamContextFactory _jsContextFactory;
        private AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private NullLogger _logger;
        private NatsSubscription _subscription;

        [SetUp]
        public void SetUp()
        {
            var url = Environment.GetEnvironmentVariable("NIMBUS_TEST_NATS");
            if (string.IsNullOrWhiteSpace(url)) Assert.Ignore("Set NIMBUS_TEST_NATS to run NATS tests.");

            _logger = new NullLogger();
            var configuration = new NatsTransportConfiguration().WithUrl(url).WithJetStream();
            _connectionFactory = new NatsConnectionFactory(configuration, _logger);
            _jsContextFactory = new NatsJetStreamContextFactory(_connectionFactory);
            _autoDeleteOnIdle = new AutoDeleteOnIdleSetting {Value = TimeSpan.FromMinutes(7)};
            _subscription = new NatsSubscription($"t.idle.{Guid.NewGuid():N}", $"sub.{Guid.NewGuid():N}");
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_jsContextFactory != null)
            {
                foreach (var stream in await _jsContextFactory.ListStreamsAsync())
                {
                    if (stream.Config.Name != null && stream.Config.Subjects!.Any(s => s.StartsWith(_subscription.TopicPath, StringComparison.Ordinal)))
                        await _jsContextFactory.DeleteStreamAsync(stream.Config.Name);
                }
            }

            _connectionFactory?.Dispose();
        }

        [Test]
        public async Task TopicConsumersAreCreatedWithAnInactiveThreshold()
        {
            var receiver = new NatsJetStreamTopicReceiver(_subscription,
                                                          _jsContextFactory,
                                                          new JsonSerializer(),
                                                          _autoDeleteOnIdle,
                                                          new ConcurrentHandlerLimitSetting(),
                                                          new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting()),
                                                          _logger);
            await receiver.Start(msg => Task.CompletedTask);
            try
            {
                var consumers = new List<(string Stream, TimeSpan Threshold)>();
                foreach (var stream in await _jsContextFactory.ListStreamsAsync())
                {
                    if (!stream.Config.Subjects!.Any(s => s.StartsWith(_subscription.TopicPath, StringComparison.Ordinal))) continue;
                    var js = await _jsContextFactory.GetContextAsync();
                    await foreach (var consumer in js.ListConsumersAsync(stream.Config.Name!))
                        consumers.Add((stream.Config.Name!, consumer.Info.Config.InactiveThreshold));
                }

                consumers.Count.ShouldBe(2); // main + retry
                consumers.ShouldAllBe(c => c.Threshold == _autoDeleteOnIdle.Value);
            }
            finally
            {
                receiver.Dispose();
            }
        }

        [Test]
        public async Task TheTopicStreamKeepsMessagesNoLongerThanTheIdleWindowWhetherTheSenderOrReceiverCreatedIt()
        {
            var sender = new NatsJetStreamTopicSender(_subscription.TopicPath, _jsContextFactory, new JsonSerializer(), _autoDeleteOnIdle);
            await sender.Send(new NimbusMessage(_subscription.TopicPath));
            (await TopicStreamMaxAge()).ShouldBe(_autoDeleteOnIdle.Value);

            // A second process (fresh stream cache) with a receiver must agree, or the stream config would flip-flop.
            var receiverFactory = new NatsJetStreamContextFactory(_connectionFactory);
            var receiver = new NatsJetStreamTopicReceiver(_subscription,
                                                          receiverFactory,
                                                          new JsonSerializer(),
                                                          _autoDeleteOnIdle,
                                                          new ConcurrentHandlerLimitSetting(),
                                                          new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting()),
                                                          _logger);
            await receiver.Start(msg => Task.CompletedTask);
            try
            {
                (await TopicStreamMaxAge()).ShouldBe(_autoDeleteOnIdle.Value);
            }
            finally
            {
                receiver.Dispose();
            }
        }

        [Test]
        public async Task TheReaperDeletesAConsumerlessRetryStreamOnlyAfterTwoSweeps()
        {
            var retrySubject = $"{_subscription.TopicPath}.sub.retry";
            var streamName = $"retry_{Guid.NewGuid():N}";
            await _jsContextFactory.EnsureStreamAsync(streamName, retrySubject);
            var reaper = new NatsJetStreamRetryStreamReaper(_jsContextFactory, _autoDeleteOnIdle, _logger);

            await reaper.ReapOnce();
            (await StreamExists(streamName)).ShouldBeTrue();

            await reaper.ReapOnce();
            (await StreamExists(streamName)).ShouldBeFalse();
        }

        [Test]
        public async Task TheReaperKeepsRetryStreamsThatStillHaveAConsumer()
        {
            var retrySubject = $"{_subscription.TopicPath}.sub.retry";
            var streamName = $"retry_{Guid.NewGuid():N}";
            await _jsContextFactory.EnsureStreamAsync(streamName, retrySubject);
            await _jsContextFactory.EnsureConsumerAsync(streamName, new NATS.Client.JetStream.Models.ConsumerConfig {Name = "c", DurableName = "c", FilterSubject = retrySubject});
            var reaper = new NatsJetStreamRetryStreamReaper(_jsContextFactory, _autoDeleteOnIdle, _logger);

            await reaper.ReapOnce();
            await reaper.ReapOnce();

            (await StreamExists(streamName)).ShouldBeTrue();
        }

        [Test]
        public async Task TheReaperIgnoresStreamsThatAreNotRetryStreams()
        {
            var streamName = $"plain_{Guid.NewGuid():N}";
            await _jsContextFactory.EnsureStreamAsync(streamName, _subscription.TopicPath);
            var reaper = new NatsJetStreamRetryStreamReaper(_jsContextFactory, _autoDeleteOnIdle, _logger);

            await reaper.ReapOnce();
            await reaper.ReapOnce();

            (await StreamExists(streamName)).ShouldBeTrue();
        }

        private async Task<TimeSpan> TopicStreamMaxAge()
        {
            var stream = (await _jsContextFactory.ListStreamsAsync()).Single(s => s.Config.Subjects!.Contains(_subscription.TopicPath));
            return stream.Config.MaxAge;
        }

        private async Task<bool> StreamExists(string name)
        {
            return (await _jsContextFactory.ListStreamsAsync()).Any(s => s.Config.Name == name);
        }
    }
}
