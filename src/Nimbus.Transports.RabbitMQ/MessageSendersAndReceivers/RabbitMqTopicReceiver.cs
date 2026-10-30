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
        private readonly string _queueName;
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
            _queueName = subscription.QueueName;
            _autoDeleteOnIdle = autoDeleteOnIdle;
        }

        protected override string ConsumeQueue => _queueName;
        protected override string ReceiverDescription => $"topic {_topicPath} subscription queue {_queueName}";

        protected override async Task DeclareTopologyAsync(IChannel channel)
        {
            await channel.ExchangeDeclareAsync(_topicPath, type: "fanout", durable: true, autoDelete: false);
            channel = await DeclareSubscriptionQueueAsync(channel);
            // Immediate publishes: fanout exchange delivers to all subscriptions
            await channel.QueueBindAsync(_queueName, _topicPath, routingKey: "");
            // Targeted retries: delayed exchange routes to this specific subscription queue
            await channel.QueueBindAsync(_queueName, RabbitMqConnectionManager.DelayedExchangeName, routingKey: _queueName);
            // Delayed publishes: TopicSender routes through the delayed exchange with the topic path as routing key
            await channel.QueueBindAsync(_queueName, RabbitMqConnectionManager.DelayedExchangeName, routingKey: _topicPath);
        }

        private async Task<IChannel> DeclareSubscriptionQueueAsync(IChannel channel)
        {
            channel = await RabbitMqSubscriptionQueue.DeclareAsync(_connectionManager, channel, _queueName, _autoDeleteOnIdle.Value, _logger);
            _channel = channel;
            return channel;
        }

        protected override void AfterDeserialize(NimbusMessage message)
        {
            // Tag so retries route directly to this subscription queue, bypassing the fanout
            message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _queueName;
        }
    }
}
