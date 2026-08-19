using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
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
            var subscribers = database.SetMembers(subscribersRedisKey)
                                      .Select(s => s.ToString())
                                      .ToArray();

            await subscribers
                .Select(subscriberPath => Task.Run(() =>
                                                   {
                                                       // A dead subscriber's liveness key has expired but its Set membership hasn't
                                                       // been reaped yet. Don't push another message into a list nobody will ever read.
                                                       var aliveKey = Subscription.SubscriberAliveRedisKeyFor(subscriberPath);
                                                       if (!database.KeyExists(aliveKey))
                                                       {
                                                           database.SetRemove(subscribersRedisKey, subscriberPath);
                                                           database.KeyDelete(subscriberPath);
                                                           return;
                                                       }

                                                       var clone = (NimbusMessage) _serializer.Deserialize(_serializer.Serialize(message), typeof (NimbusMessage));
                                                       clone.DeliverTo = subscriberPath;
                                                       var serialized = _serializer.Serialize(clone);
                                                       database.ListRightPush(subscriberPath, serialized);
                                                       database.Publish(subscriberPath, string.Empty);
                                                   }).ConfigureAwaitFalse())
                .WhenAll();
        }
    }
}