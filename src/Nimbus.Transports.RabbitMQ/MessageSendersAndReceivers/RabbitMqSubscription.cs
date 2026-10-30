using Nimbus.Infrastructure;

namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    internal record RabbitMqSubscription(string TopicPath, string SubscriptionName)
    {
        // RabbitMQ rejects queue names longer than 255 bytes. Paths are sanitised to ASCII, so characters == bytes.
        private const int MaxQueueNameLength = 255;

        // A subscription name identifies the handler, not the topic, so a handler of several event types has the same
        // subscription name on each topic. Scoping the queue by topic keeps each topic's messages on its own queue;
        // a shared queue would hand events to receivers that have no handler for them.
        public string QueueName { get; } = PathFactory.Shorten($"{TopicPath}.{SubscriptionName}", MaxQueueNameLength);
    }
}
