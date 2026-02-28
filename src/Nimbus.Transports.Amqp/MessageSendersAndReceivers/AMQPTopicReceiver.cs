using System;
using System.Threading;
using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.AMQP.MessageConversion;
using Nimbus.Transports.AMQP.QueueManagement;
using static Nimbus.Infrastructure.MessagePropertyKeys;

namespace Nimbus.Transports.AMQP.MessageSendersAndReceivers
{
    internal class AMQPTopicReceiver : ThrottlingMessageReceiver
    {
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private readonly string _durableSubscriptionName;
        private readonly IFilterCondition _filterCondition;
        private readonly IQueueManager _queueManager;
        private readonly INmsMessageFactory _messageFactory;
        private readonly ILogger _logger;
        private ISession _session;
        private IMessageConsumer _consumer;

        public AMQPTopicReceiver(Subscription subscription,
                                     IFilterCondition filterCondition,
                                     IQueueManager queueManager,
                                     INmsMessageFactory messageFactory,
                                     ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                     IGlobalHandlerThrottle globalHandlerThrottle,
                                     ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _topicPath = subscription.TopicPath;
            _subscriptionName = subscription.SubscriptionName;
            _durableSubscriptionName = $"{subscription.SubscriptionName}.{subscription.TopicPath}";
            _filterCondition = filterCondition;
            _queueManager = queueManager;
            _messageFactory = messageFactory;
            _logger = logger;
        }

        protected override async Task WarmUp()
        {
            _logger.Debug("Warming up topic receiver for {TopicPath} with subscription {SubscriptionName}",
                _topicPath, _subscriptionName);

            _session = await _queueManager.CreateSession(AcknowledgementMode.ClientAcknowledge);
            var topic = await _queueManager.GetTopic(_session, _topicPath);

            var selector = BuildMessageSelector(_filterCondition);

            if (string.IsNullOrWhiteSpace(selector))
            {
                _consumer = await _session.CreateDurableConsumerAsync(topic, _durableSubscriptionName, null, false);
            }
            else
            {
                _consumer = await _session.CreateDurableConsumerAsync(topic, _durableSubscriptionName, selector, false);
            }

            _logger.Info("Topic receiver for {TopicPath} with subscription {SubscriptionName} is ready",
                _topicPath, _subscriptionName);
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            try
            {
                // Use a timeout to allow for cancellation checks
                var nmsMessage = await Task.Run(() => _consumer.Receive(TimeSpan.FromSeconds(1)), cancellationToken)
                    .ConfigureAwaitFalse();

                if (nmsMessage == null)
                {
                    return null;
                }

                _logger.Debug("Received message {MessageId} from topic {TopicPath} subscription {SubscriptionName}",
                    nmsMessage.NMSMessageId, _topicPath, _subscriptionName);

                var nimbusMessage = await _messageFactory.CreateNimbusMessage(nmsMessage);

                // Tag with subscription name so retries are routed back to this
                // specific subscription via the JMS selector, not broadcast to all.
                nimbusMessage.Properties[RedeliveryToSubscriptionName] = _subscriptionName;

                // Acknowledge the message after successful conversion
                await nmsMessage.AcknowledgeAsync();

                return nimbusMessage;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching message from topic {TopicPath} subscription {SubscriptionName}",
                    _topicPath, _subscriptionName);
                throw;
            }
        }

        private string BuildMessageSelector(IFilterCondition filterCondition)
        {
            // Accept original messages (property absent) or retries targeted at this subscription.
            // This mirrors the Azure Service Bus subscription filter approach.
            return $"({RedeliveryToSubscriptionName} = '{_subscriptionName}' OR {RedeliveryToSubscriptionName} IS NULL)";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger.Debug("Disposing topic receiver for {TopicPath} subscription {SubscriptionName}",
                    _topicPath, _subscriptionName);

                try
                {
                    _consumer?.Close();
                    _consumer?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Error disposing consumer for {TopicPath} subscription {SubscriptionName}",
                        _topicPath, _subscriptionName);
                }

                try
                {
                    _session?.Close();
                    _session?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Error disposing session for {TopicPath} subscription {SubscriptionName}",
                        _topicPath, _subscriptionName);
                }
            }

            base.Dispose(disposing);
        }
    }
}
