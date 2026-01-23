using System;
using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AMQP.MessageConversion;
using Nimbus.Transports.AMQP.QueueManagement;

namespace Nimbus.Transports.AMQP.MessageSendersAndReceivers
{
    internal class AMQPTopicSender : INimbusMessageSender
    {
        private readonly string _topicPath;
        private readonly IQueueManager _queueManager;
        private readonly INmsMessageFactory _messageFactory;
        private readonly ILogger _logger;

        public AMQPTopicSender(string topicPath,
                                   IQueueManager queueManager,
                                   INmsMessageFactory messageFactory,
                                   ILogger logger)
        {
            _topicPath = topicPath;
            _queueManager = queueManager;
            _messageFactory = messageFactory;
            _logger = logger;
        }

        public async Task Send(NimbusMessage message)
        {
            try
            {
                using var pooledConnection = await _queueManager.GetConnection();
                using var session = await pooledConnection.Connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
                var topic = await _queueManager.GetTopic(session, _topicPath);

                using var producer = await session.CreateProducerAsync(topic);
                var nmsMessage = await _messageFactory.CreateNmsMessage(message, session);

                _logger.Debug("Publishing message {MessageId} to topic {TopicPath}", message.MessageId, _topicPath);
                await producer.SendAsync(nmsMessage).ConfigureAwaitFalse();
                _logger.Debug("Message {MessageId} published successfully to topic {TopicPath}", message.MessageId, _topicPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to publish message {MessageId} to topic {TopicPath}", message.MessageId, _topicPath);
                throw;
            }
        }
    }
}
