namespace Nimbus.Transports.Redis.MessageSendersAndReceivers
{
    internal class Subscription
    {
        internal const string SubscriptionsPrefix = "subscriptions";

        public string TopicPath { get; }
        public string SubscriptionName { get; }
        public string TopicSubscribersRedisKey { get; }
        public string SubscriptionMessagesRedisKey { get; }
        public string SubscriberAliveRedisKey { get; }

        public Subscription(string topicPath, string subscriptionName)
        {
            TopicPath = topicPath;
            SubscriptionName = subscriptionName;
            TopicSubscribersRedisKey = TopicSubscribersRedisKeyFor(topicPath);
            SubscriptionMessagesRedisKey = $"{topicPath}.{subscriptionName}";
            SubscriberAliveRedisKey = SubscriberAliveRedisKeyFor(SubscriptionMessagesRedisKey);
        }

        public static string TopicSubscribersRedisKeyFor(string topicPath)
        {
            return $"{SubscriptionsPrefix}.{topicPath}";
        }

        public static string SubscriberAliveRedisKeyFor(string subscriptionMessagesRedisKey)
        {
            return $"{subscriptionMessagesRedisKey}.alive";
        }
    }
}