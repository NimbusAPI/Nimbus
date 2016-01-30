using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Retries;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.MessageSendersAndReceivers
{
    internal class RedisSubscriptionReceiver : RedisMessageReceiver
    {
        private readonly Subscription _subscription;
        private readonly Func<IDatabase> _databaseFunc;
        private readonly IRetry _retry;

        public RedisSubscriptionReceiver(Subscription subscription,
                                         Func<ConnectionMultiplexer> connectionMultiplexerFunc,
                                         Func<IDatabase> databaseFunc,
                                         ISerializer serializer,
                                         ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                         IGlobalHandlerThrottle globalHandlerThrottle,
                                         ILogger logger,
                                         IRetry retry)
            : base(
                subscription.SubscriptionMessagesRedisKey,
                connectionMultiplexerFunc,
                databaseFunc,
                serializer,
                concurrentHandlerLimit,
                globalHandlerThrottle,
                logger,
                retry)
        {
            _subscription = subscription;
            _databaseFunc = databaseFunc;
            _retry = retry;
        }

        protected override async Task WarmUp()
        {
            var database = _databaseFunc();
            _retry.Do(() => database.SetAdd(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey));
            await base.WarmUp();
        }
    }
}