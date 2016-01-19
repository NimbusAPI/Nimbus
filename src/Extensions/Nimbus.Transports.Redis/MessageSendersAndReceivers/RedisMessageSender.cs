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

        public RedisMessageSender(string redisKey, ISerializer serializer, Func<IDatabase> databaseFunc)
        {
            _redisKey = redisKey;
            _serializer = serializer;
            _databaseFunc = databaseFunc;
        }

        public async Task Send(NimbusMessage message)
        {
            var serialized = _serializer.Serialize(message);
            var database = _databaseFunc();
            await database.ListRightPushAsync(_redisKey, serialized);
            await database.PublishAsync(_redisKey, string.Empty);
        }
    }
}