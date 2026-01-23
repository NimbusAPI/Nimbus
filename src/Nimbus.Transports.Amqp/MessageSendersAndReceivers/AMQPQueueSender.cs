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
    internal class AMQPQueueSender : INimbusMessageSender
    {
        private readonly string _queuePath;
        private readonly IQueueManager _queueManager;
        private readonly INmsMessageFactory _messageFactory;
        private readonly ILogger _logger;

        public AMQPQueueSender(string queuePath,
                                   IQueueManager queueManager,
                                   INmsMessageFactory messageFactory,
                                   ILogger logger)
        {
            _queuePath = queuePath;
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
                var queue = await _queueManager.GetQueue(session, _queuePath);

                using var producer = session.CreateProducer(queue);
                var nmsMessage = await _messageFactory.CreateNmsMessage(message, session);

                _logger.Debug("Sending message {MessageId} to queue {QueuePath}", message.MessageId, _queuePath);
                await producer.SendAsync(nmsMessage).ConfigureAwaitFalse();
                _logger.Debug("Message {MessageId} sent successfully to queue {QueuePath}", message.MessageId, _queuePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send message {MessageId} to queue {QueuePath}", message.MessageId, _queuePath);
                throw;
            }
        }
    }
}
