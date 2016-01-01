using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.Redis.QueueManagement;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.MessageSendersAndReceivers
{
    internal class RedisMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly Queue _queue;
        private readonly Func<ConnectionMultiplexer> _connectionMultiplexerFunc;
        private readonly Func<IDatabase> _databaseFunc;
        private readonly ISerializer _serializer;

        private ISubscriber _subscriber;
        readonly SemaphoreSlim _receiveSemaphore = new SemaphoreSlim(0, int.MaxValue);
        private bool _haveFetchedAllPreExistingMessages;

        public RedisMessageReceiver(Queue queue,
                                    Func<ConnectionMultiplexer> connectionMultiplexerFunc,
                                    Func<IDatabase> databaseFunc,
                                    ISerializer serializer,
                                    ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                    IGlobalHandlerThrottle globalHandlerThrottle,
                                    ILogger logger) : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queue = queue;
            _connectionMultiplexerFunc = connectionMultiplexerFunc;
            _databaseFunc = databaseFunc;
            _serializer = serializer;
        }

        protected override Task WarmUp()
        {
            return Task.Run(() =>
                            {
                                _subscriber = _connectionMultiplexerFunc().GetSubscriber();
                                _subscriber.SubscribeAsync(_queue.QueuePath, OnNotificationReceived);
                            }).ConfigureAwaitFalse();
        }

        protected override void Dispose(bool disposing)
        {
            _subscriber?.UnsubscribeAll();
            base.Dispose(disposing);
        }

        private void OnNotificationReceived(RedisChannel redisChannel, RedisValue redisValue)
        {
            _receiveSemaphore.Release();
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            if (_haveFetchedAllPreExistingMessages) await _receiveSemaphore.WaitAsync(cancellationToken);

            var database = _databaseFunc();
            var redisValue = database.ListLeftPop(_queue.QueuePath);
            if (!redisValue.HasValue)
            {
                _haveFetchedAllPreExistingMessages = true;
                return null;
            }

            var message = (NimbusMessage) _serializer.Deserialize(redisValue, typeof (NimbusMessage));
            return message;
        }
    }
}