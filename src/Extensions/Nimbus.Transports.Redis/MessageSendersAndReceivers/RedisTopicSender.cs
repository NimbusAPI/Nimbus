using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.MessageSendersAndReceivers
{
    internal class RedisTopicSender : INimbusMessageSender
    {
        private readonly string _topicPath;
        private readonly ISerializer _serializer;
        private readonly Func<IDatabase> _databaseFunc;

        public RedisTopicSender(string topicPath, ISerializer serializer, Func<IDatabase> databaseFunc)
        {
            _topicPath = topicPath;
            _serializer = serializer;
            _databaseFunc = databaseFunc;
        }

        public Task Send(NimbusMessage message)
        {
            return Task.Run(async () =>
                                  {
                                      var serialized = _serializer.Serialize(message);
                                      var database = _databaseFunc();
                                      var subscribersRedisKey = Subscription.TopicSubscribersRedisKeyFor(_topicPath);
                                      var subscribers = database.SetMembers(subscribersRedisKey)
                                                                .Select(s => s.ToString())
                                                                .ToArray();

                                      await subscribers
                                          .Select(s => Task.Run(() =>
                                                                {
                                                                    database.ListRightPush(s, serialized);
                                                                    database.Publish(s, string.Empty);
                                                                }).ConfigureAwaitFalse())
                                          .WhenAll();
                                  }).ConfigureAwaitFalse();
        }
    }
}