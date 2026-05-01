namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal class NatsSubscription
    {
        public string TopicPath { get; }
        public string SubscriptionName { get; }

        public NatsSubscription(string topicPath, string subscriptionName)
        {
            TopicPath = topicPath;
            SubscriptionName = subscriptionName;
        }
    }
}
