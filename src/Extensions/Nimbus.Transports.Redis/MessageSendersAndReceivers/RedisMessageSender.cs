using System;
using System.Threading.Tasks;
using Nimbus.Extensions;
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

        public Task Send(NimbusMessage message)
        {
            return Task.Run(() =>
                            {
                                var serialized = _serializer.Serialize(message);
                                var database = _databaseFunc();
                                database.ListRightPush(_redisKey, serialized);
                                database.Publish(_redisKey, string.Empty);
                            }).ConfigureAwaitFalse();
        }
    }
}