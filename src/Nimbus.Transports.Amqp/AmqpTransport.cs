using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.Amqp.ConnectionManagement;
using Nimbus.Transports.Amqp.Messages;
using Nimbus.Transports.Amqp.SendersAndReceivers;

namespace Nimbus.Transports.Amqp
{
    internal class AmqpTransport : INimbusTransport
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger _logger;
        private readonly IMessageFactory _messageFactory;

        private readonly ThreadSafeDictionary<string, INimbusMessageSender> _queueMessageSenders = new ThreadSafeDictionary<string, INimbusMessageSender>();
        private readonly ThreadSafeDictionary<string, INimbusMessageReceiver> _queueMessageReceivers = new ThreadSafeDictionary<string, INimbusMessageReceiver>();
        private readonly ThreadSafeDictionary<string, INimbusMessageSender> _topicMessageSenders = new ThreadSafeDictionary<string, INimbusMessageSender>();
        private readonly ThreadSafeDictionary<string, INimbusMessageReceiver> _topicMessageReceivers = new ThreadSafeDictionary<string, INimbusMessageReceiver>();

        public AmqpTransport(
            IConnectionManager connectionManager,
            ILogger logger,
            IMessageFactory messageFactory)
        {
            _connectionManager = connectionManager;
            _logger = logger;
            _messageFactory = messageFactory;
        }

        public Task TestConnection()
        {
            return Task.CompletedTask;
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            _logger.Debug("AMQP: Attempting to get Queue {QueuePath} sender.", queuePath);
            return _queueMessageSenders.GetOrAdd(queuePath, CreateMessageSender);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            _logger.Debug("AMQP: Attempting to get Topic {TopicPath} sender.", topicPath);
            return _topicMessageSenders.GetOrAdd(topicPath, CreateTopicSender);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            _logger.Debug("AMQP: Attempting to get Queue {QueuePath} receiver.", queuePath);
            return _queueMessageReceivers.GetOrAdd(queuePath, CreateQueueReceiver);
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            _logger.Debug("AMQP: Attempting to get {TopicPath} for {SubscriptionName} receiver.", topicPath, subscriptionName);
            return _topicMessageReceivers.GetOrAdd(topicPath, CreateTopicReceiver);
        }

        private INimbusMessageSender CreateMessageSender(string queuePath)
        {
            return new AmqpQueueSender(_connectionManager, _messageFactory, _logger, queuePath);
        }

        private INimbusMessageSender CreateTopicSender(string topicPath)
        {
            return new AmqpQueueSender(_connectionManager, _messageFactory, _logger, topicPath);
        }

        private INimbusMessageReceiver CreateQueueReceiver(string queuePath)
        {
            return new AmqpQueueReceiver(_connectionManager,
                                         queuePath,
                                         _messageFactory,
                                         new ConcurrentHandlerLimitSetting {Value = 10},
                                         new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting
                                                                   {Value = 10}),
                                         _logger);
        }

        private INimbusMessageReceiver CreateTopicReceiver(string topicPath)
        {
            return new AmqpTopicReceiver(_connectionManager,
                                         _messageFactory,
                                         topicPath,
                                         new ConcurrentHandlerLimitSetting {Value = 10},
                                         new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting
                                                                   {Value = 10}),
                                         _logger);
        }
    }
}