using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using RabbitMQ.Client;

namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    internal class RabbitMqTopicReceiver : RabbitMqReceiverBase
    {
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;

        public RabbitMqTopicReceiver(RabbitMqSubscription subscription,
                                      IFilterCondition filterCondition,
                                      RabbitMqConnectionManager connectionManager,
                                      RabbitMqMessageConverter messageConverter,
                                      AutoDeleteOnIdleSetting autoDeleteOnIdle,
                                      ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                      IGlobalHandlerThrottle globalHandlerThrottle,
                                      ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, connectionManager, messageConverter, logger)
        {
            _topicPath = subscription.TopicPath;
            _subscriptionName = subscription.SubscriptionName;
            _autoDeleteOnIdle = autoDeleteOnIdle;
        }

        protected override string ConsumeQueue => _subscriptionName;
        protected override string ReceiverDescription => $"topic {_topicPath} subscription {_subscriptionName}";

        protected override async Task DeclareTopologyAsync(IChannel channel)
        {
            await channel.ExchangeDeclareAsync(_topicPath, type: "fanout", durable: true, autoDelete: false);
            channel = await DeclareSubscriptionQueueAsync(channel);
            // Immediate publishes: fanout exchange delivers to all subscriptions
            await channel.QueueBindAsync(_subscriptionName, _topicPath, routingKey: "");
            // Targeted retries: delayed exchange routes to this specific subscription queue
            await channel.QueueBindAsync(_subscriptionName, RabbitMqConnectionManager.DelayedExchangeName, routingKey: _subscriptionName);
            // Delayed publishes: TopicSender routes through the delayed exchange with the topic path as routing key
            await channel.QueueBindAsync(_subscriptionName, RabbitMqConnectionManager.DelayedExchangeName, routingKey: _topicPath);
        }

        private async Task<IChannel> DeclareSubscriptionQueueAsync(IChannel channel)
        {
            channel = await RabbitMqSubscriptionQueue.DeclareAsync(_connectionManager, channel, _subscriptionName, _autoDeleteOnIdle.Value, _logger);
            _channel = channel;
            return channel;
        }

        protected override void AfterDeserialize(NimbusMessage message)
        {
            // Tag so retries route directly to this subscription queue, bypassing the fanout
            message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscriptionName;
        }
    }
}
