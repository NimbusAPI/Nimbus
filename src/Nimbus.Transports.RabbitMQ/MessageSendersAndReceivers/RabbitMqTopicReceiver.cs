using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

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

        // x-expires makes the broker delete the queue (and its bindings) once it has had no consumers for the
        // idle window, which cleans up the per-instance queues orphaned by every restart.
        private async Task<IChannel> DeclareSubscriptionQueueAsync(IChannel channel)
        {
            var arguments = new Dictionary<string, object> {{"x-expires", (int) _autoDeleteOnIdle.Value.TotalMilliseconds}};

            try
            {
                await channel.QueueDeclareAsync(_subscriptionName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
                return channel;
            }
            catch (OperationInterruptedException exc) when (exc.ShutdownReason?.ReplyCode == 406)
            {
                // The queue predates idle cleanup and was declared without x-expires. The broker closed the channel,
                // so carry on over a new one and attach to the existing queue as-is.
                _logger.Warn("Queue {Queue} already exists without x-expires, so it will not be removed when idle. Delete it, or apply an 'expires' policy, to enable idle cleanup.", _subscriptionName);

                try { channel.Dispose(); } catch { }
                channel = await _connectionManager.CreateChannelAsync();
                _channel = channel;
                await channel.QueueDeclarePassiveAsync(_subscriptionName);
                return channel;
            }
        }

        protected override void AfterDeserialize(NimbusMessage message)
        {
            // Tag so retries route directly to this subscription queue, bypassing the fanout
            message.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscriptionName;
        }
    }
}
