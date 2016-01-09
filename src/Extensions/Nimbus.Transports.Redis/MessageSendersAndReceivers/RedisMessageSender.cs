using System;
using System.Threading.Tasks;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.MessageSendersAndReceivers
{
    internal class RedisMessageSender : INimbusMessageSender
    {
        private readonly string _redisKey;
        private readonly ISerializer _serializer;
        private readonly Func<IDatabase> _databaseFunc;
        private readonly ILogger _logger;

        public RedisMessageSender(string redisKey, ISerializer serializer, Func<IDatabase> databaseFunc, ILogger logger)
        {
            _redisKey = redisKey;
            _serializer = serializer;
            _databaseFunc = databaseFunc;
            _logger = logger;
        }

        public async Task Send(NimbusMessage message)
        {
            var serialized = _serializer.Serialize(message);
            var database = _databaseFunc();

            var pushResult = await database.ListRightPushAsync(_redisKey, serialized);
            _logger.Debug($"Redis {nameof(database.ListRightPushAsync)} to {{RedisKey}} returned {{RedisReturnCode}}", _redisKey, pushResult);

            var publishResult = await database.PublishAsync(_redisKey, string.Empty);
            _logger.Debug($"Redis {nameof(database.PublishAsync)} to {{RedisKey}} returned {{RedisReturnCode}}", _redisKey, publishResult);
        }
    }
}