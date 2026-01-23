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
using Nimbus.Transports.AMQP.ConnectionManagement;
using Nimbus.Transports.AMQP.MessageConversion;
using Nimbus.Transports.AMQP.QueueManagement;

namespace Nimbus.Transports.AMQP.MessageSendersAndReceivers
{
    internal class AMQPTopicReceiver : ThrottlingMessageReceiver
    {
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private readonly IFilterCondition _filterCondition;
        private readonly IQueueManager _queueManager;
        private readonly INmsMessageFactory _messageFactory;
        private readonly AMQPTransportConfiguration _configuration;
        private readonly ILogger _logger;
        private PooledConnection _pooledConnection;
        private ISession _session;
        private IMessageConsumer _consumer;

        public AMQPTopicReceiver(string topicPath,
                                     string subscriptionName,
                                     IFilterCondition filterCondition,
                                     IQueueManager queueManager,
                                     INmsMessageFactory messageFactory,
                                     AMQPTransportConfiguration configuration,
                                     ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                     IGlobalHandlerThrottle globalHandlerThrottle,
                                     ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _topicPath = topicPath;
            _subscriptionName = subscriptionName;
            _filterCondition = filterCondition;
            _queueManager = queueManager;
            _messageFactory = messageFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task WarmUp()
        {
            _logger.Debug("Warming up topic receiver for {TopicPath} with subscription {SubscriptionName}",
                _topicPath, _subscriptionName);

            _pooledConnection = await _queueManager.GetConnection();

            // For durable subscriptions, we need to set the client ID
            if (!string.IsNullOrWhiteSpace(_configuration.ClientId))
            {
                _pooledConnection.Connection.ClientId = _configuration.ClientId;
            }

            _session = await _pooledConnection.Connection.CreateSessionAsync(AcknowledgementMode.ClientAcknowledge);
            var topic = await _queueManager.GetTopic(_session, _topicPath);

            // Create durable subscriber with subscription name
            // Note: NMS AMQP creates durable subscriptions when subscription name is provided
            var selector = BuildMessageSelector(_filterCondition);

            if (string.IsNullOrWhiteSpace(selector))
            {
                _consumer = await _session.CreateDurableConsumerAsync(topic, _subscriptionName, null, false);
            }
            else
            {
                _consumer = await _session.CreateDurableConsumerAsync(topic, _subscriptionName, selector, false);
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
            // AMQP supports JMS selectors
            // For now, we'll return null (no filtering) since Nimbus filtering is typically done at the handler level
            // This could be extended to convert Nimbus filter conditions to JMS selectors
            return null;
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

                try
                {
                    _pooledConnection?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Error returning pooled connection for {TopicPath} subscription {SubscriptionName}",
                        _topicPath, _subscriptionName);
                }
            }

            base.Dispose(disposing);
        }
    }
}
