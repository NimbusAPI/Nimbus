namespace Nimbus.Transports.InProcess.QueueManagement
{
    public class Subscription
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