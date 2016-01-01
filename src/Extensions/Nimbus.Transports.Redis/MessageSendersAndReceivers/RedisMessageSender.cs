using System;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.Redis.QueueManagement;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.MessageSendersAndReceivers
{
    internal class RedisMessageSender : INimbusMessageSender
    {
        private readonly ISerializer _serializer;
        private readonly Queue _queue;
        private readonly Func<IDatabase> _databaseFunc;

        public RedisMessageSender(ISerializer serializer, Queue queue, Func<IDatabase> databaseFunc)
        {
            _serializer = serializer;
            _queue = queue;
            _databaseFunc = databaseFunc;
        }

        public Task Send(NimbusMessage message)
        {
            return Task.Run(() =>
                            {
                                var serialized = _serializer.Serialize(message);
                                var database = _databaseFunc();
                                database.ListRightPush(_queue.QueuePath, serialized);

                                database.Publish(_queue.QueuePath, string.Empty);
                            }).ConfigureAwaitFalse();
        }
    }
}