namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    internal record RabbitMqSubscription(string TopicPath, string SubscriptionName);
}
