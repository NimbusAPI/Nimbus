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

        public async Task Send(NimbusMessage message)
        {
            var database = _databaseFunc();

            var subscribersRedisKey = Subscription.TopicSubscribersRedisKeyFor(_topicPath);
            var subscribers = (await database.SetMembersAsync(subscribersRedisKey))
                .Select(s => s.ToString())
                .ToArray();

            await subscribers
                .Select(subscriberPath => Task.Run(async () =>
                                                         {
                                                             var clone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
                                                             clone.DeliverTo = subscriberPath;
                                                             var serialized = _serializer.Serialize(clone);
                                                             await database.ListRightPushAsync(subscriberPath, serialized);
                                                             while (true)
                                                             {
                                                                 var publishResult = await database.PublishAsync(subscriberPath, string.Empty);
                                                                 if (publishResult > 0) break;
                                                             }
                                                         }).ConfigureAwaitFalse())
                .WhenAll();
        }
    }
}