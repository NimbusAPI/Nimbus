namespace Nimbus.Transports.Postgres.MessageSendersAndReceivers
{
    internal class PostgresSubscription
    {
        public string TopicPath { get; }
        public string SubscriptionName { get; }

        public PostgresSubscription(string topicPath, string subscriptionName)
        {
            TopicPath = topicPath;
            SubscriptionName = subscriptionName;
        }

        /// <summary>
        /// The queue name that this subscription's messages are delivered to.
        /// Combines topic and subscription name to form a unique destination.
        /// </summary>
        public string SubscriberQueueName => $"{TopicPath}/{SubscriptionName}";
    }
}
