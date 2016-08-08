using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Routing;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.DeadLetterOffice
{
    internal class RedisDeadLetterOffice : IDeadLetterOffice
    {
        private readonly string _deadLetterOfficeRedisKey;

        private readonly Func<IDatabase> _databaseFunc;
        private readonly ISerializer _serializer;

        public RedisDeadLetterOffice(Func<IDatabase> databaseFunc, IPathFactory pathFactory, ISerializer serializer)
        {
            _databaseFunc = databaseFunc;
            _serializer = serializer;
            _deadLetterOfficeRedisKey = pathFactory.DeadLetterOfficePath();
        }

        public Task<NimbusMessage> Peek()
        {
            return Task.Run(() =>
                            {
                                var database = _databaseFunc();
                                var redisValues = database.ListRange(_deadLetterOfficeRedisKey, 0, 1);

                                var redisValue = redisValues.FirstOrDefault();
                                if (!redisValue.HasValue) return null;

                                var message = (NimbusMessage) _serializer.Deserialize(redisValue.ToString(), typeof(NimbusMessage));
                                return message;
                            }).ConfigureAwaitFalse();
        }

        public Task<NimbusMessage> Pop()
        {
            return Task.Run(() =>
                            {
                                var database = _databaseFunc();
                                var redisValue = database.ListLeftPop(_deadLetterOfficeRedisKey);
                                if (!redisValue.HasValue) return null;

                                var message = (NimbusMessage) _serializer.Deserialize(redisValue.ToString(), typeof(NimbusMessage));
                                return message;
                            }).ConfigureAwaitFalse();
        }

        public Task Post(NimbusMessage message)
        {
            return Task.Run(() =>
                            {
                                var database = _databaseFunc();
                                var serialized = _serializer.Serialize(message);
                                database.ListRightPush(_deadLetterOfficeRedisKey, serialized);
                            }).ConfigureAwaitFalse();
        }

        public Task<int> Count()
        {
            return Task.Run(() =>
                            {
                                var database = _databaseFunc();
                                var count = database.ListLength(_deadLetterOfficeRedisKey);
                                return (int) count;
                            }).ConfigureAwaitFalse();
        }
    }
}