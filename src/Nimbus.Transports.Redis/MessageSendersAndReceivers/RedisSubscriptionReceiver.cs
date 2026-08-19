using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Retries;
using Nimbus.InfrastructureContracts;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.MessageSendersAndReceivers
{
    internal class RedisSubscriptionReceiver : RedisMessageReceiver
    {
        private readonly Subscription _subscription;
        private readonly Func<IDatabase> _databaseFunc;
        private readonly IRetry _retry;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;

        // Refreshed well inside the TTL so a receiver never sits an entire heartbeat interval away
        // from expiry — a missed poll or two shouldn't be enough to make a live subscriber look dead.
        private readonly TimeSpan _heartbeatRefreshInterval;
        private DateTime _nextHeartbeatRefreshUtc = DateTime.MinValue;

        public RedisSubscriptionReceiver(Subscription subscription,
            Func<ConnectionMultiplexer> connectionMultiplexerFunc,
            Func<IDatabase> databaseFunc,
            ISerializer serializer,
            ConcurrentHandlerLimitSetting concurrentHandlerLimit,
            IGlobalHandlerThrottle globalHandlerThrottle,
            ILogger logger,
            IRetry retry,
            AutoDeleteOnIdleSetting autoDeleteOnIdle)
            : base(
                subscription.SubscriptionMessagesRedisKey,
                connectionMultiplexerFunc,
                databaseFunc,
                serializer,
                concurrentHandlerLimit,
                globalHandlerThrottle,
                logger)
        {
            _subscription = subscription;
            _databaseFunc = databaseFunc;
            _retry = retry;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            _heartbeatRefreshInterval = TimeSpan.FromTicks(autoDeleteOnIdle.Value.Ticks / 4);
        }

        protected override async Task WarmUp()
        {
            var database = _databaseFunc();
            await _retry
                .DoAsync(() => database.SetAddAsync(_subscription.TopicSubscribersRedisKey,
                    _subscription.SubscriptionMessagesRedisKey)).ConfigureAwaitFalse();
            RefreshHeartbeat(database);
            await base.WarmUp().ConfigureAwaitFalse();
        }

        protected override void OnPoll()
        {
            var now = DateTime.UtcNow;
            if (now < _nextHeartbeatRefreshUtc) return;

            RefreshHeartbeat(_databaseFunc());
            _nextHeartbeatRefreshUtc = now + _heartbeatRefreshInterval;
        }

        private void RefreshHeartbeat(IDatabase database)
        {
            _retry.Do(() => database.StringSet(_subscription.SubscriberAliveRedisKey, true, _autoDeleteOnIdle.Value));
        }
    }
}