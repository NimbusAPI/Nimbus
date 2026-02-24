namespace Nimbus.Transports.AMQP.QueueManagement
{
    internal class Subscription
    {
        public string TopicPath { get; }
        public string SubscriptionName { get; }

        public Subscription(string topicPath, string subscriptionName)
        {
            TopicPath = topicPath;
            SubscriptionName = subscriptionName;
        }
    }
}
