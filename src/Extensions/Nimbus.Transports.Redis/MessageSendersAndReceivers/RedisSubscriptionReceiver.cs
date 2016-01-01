using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.MessageSendersAndReceivers
{
    internal class RedisSubscriptionReceiver : RedisMessageReceiver
    {
        private readonly Subscription _subscription;
        private readonly Func<IDatabase> _databaseFunc;

        public RedisSubscriptionReceiver(Subscription subscription,
                                         Func<ConnectionMultiplexer> connectionMultiplexerFunc,
                                         Func<IDatabase> databaseFunc,
                                         ISerializer serializer,
                                         ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                         IGlobalHandlerThrottle globalHandlerThrottle,
                                         ILogger logger)
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
        }

        protected override async Task WarmUp()
        {
            await _databaseFunc().SetAddAsync(_subscription.TopicSubscribersRedisKey, _subscription.SubscriptionMessagesRedisKey);
            await base.WarmUp();
        }
    }
}