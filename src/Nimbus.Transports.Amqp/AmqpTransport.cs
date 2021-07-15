using System;
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
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        private readonly IMessageFactory _messageFactory;

        private readonly ThreadSafeDictionary<string, INimbusMessageSender> _queueMessageSenders = new ThreadSafeDictionary<string, INimbusMessageSender>();
        private readonly ThreadSafeDictionary<string, INimbusMessageReceiver> _queueMessageReceivers = new ThreadSafeDictionary<string, INimbusMessageReceiver>();
        private readonly ThreadSafeDictionary<string, INimbusMessageSender> _topicMessageSenders = new ThreadSafeDictionary<string, INimbusMessageSender>();
        private readonly ThreadSafeDictionary<string, INimbusMessageReceiver> _topicMessageReceivers = new ThreadSafeDictionary<string, INimbusMessageReceiver>();

        public AmqpTransport(IConnectionManager connectionManager, ISerializer serializer, ILogger logger, IMessageFactory messageFactory)
        {
            _connectionManager = connectionManager;
            _serializer = serializer;
            _logger = logger;
            _messageFactory = messageFactory;
        }

        public Task TestConnection()
        {
            return Task.CompletedTask;
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return _queueMessageSenders.GetOrAdd(queuePath, CreateQueueSender);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return _queueMessageReceivers.GetOrAdd(queuePath, CreateQueueReceiver);
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            throw new NotImplementedException();
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            throw new NotImplementedException();
        }

        private INimbusMessageSender CreateQueueSender(string queuePath)
        {
            return new AmqpMessageSender(_connectionManager, queuePath, _messageFactory);
        }
        
        public INimbusMessageReceiver CreateQueueReceiver(string queuePath)
        {
            return new AmqpMessageReceiver(_connectionManager,
                                           queuePath,
                                           _messageFactory,
                                           new ConcurrentHandlerLimitSetting {Value = 10},
                                           new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting
                                                                     {Value = 10}),
                                           _logger);
        }
    }
}