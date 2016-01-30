using System;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.DeadLetterOffice
{
    internal class RedisDeadLetterOffice : IDeadLetterOffice
    {
        private const string _deadLetterOfficeRedisKey = "deadletteroffice";

        private readonly Func<IDatabase> _databaseFunc;
        private readonly ISerializer _serializer;

        public RedisDeadLetterOffice(Func<IDatabase> databaseFunc, ISerializer serializer)
        {
            _databaseFunc = databaseFunc;
            _serializer = serializer;
        }

        public async Task<NimbusMessage> Peek()
        {
            var database = _databaseFunc();
            var redisValues = await database.ListRangeAsync(_deadLetterOfficeRedisKey, 0, 1);

            var redisValue = redisValues.FirstOrDefault();
            if (!redisValue.HasValue) return null;

            var message = (NimbusMessage) _serializer.Deserialize(redisValue.ToString(), typeof (NimbusMessage));
            return message;
        }

        public async Task<NimbusMessage> Pop()
        {
            var database = _databaseFunc();
            var redisValue = await database.ListLeftPopAsync(_deadLetterOfficeRedisKey);
            if (!redisValue.HasValue) return null;

            var message = (NimbusMessage) _serializer.Deserialize(redisValue.ToString(), typeof (NimbusMessage));
            return message;
        }

        public async Task Post(NimbusMessage message)
        {
            var database = _databaseFunc();
            var serialized = _serializer.Serialize(message);
            await database.ListRightPushAsync(_deadLetterOfficeRedisKey, serialized);
        }

        public async Task<int> Count()
        {
            var database = _databaseFunc();
            var count = await database.ListLengthAsync(_deadLetterOfficeRedisKey);
            return (int) count;
        }
    }
}