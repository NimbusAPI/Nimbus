using System;
using System.Threading;
using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AMQP.MessageConversion;
using Nimbus.Transports.AMQP.QueueManagement;

namespace Nimbus.Transports.AMQP.MessageSendersAndReceivers
{
    internal class AMQPQueueReceiver : ThrottlingMessageReceiver
    {
        private readonly string _queuePath;
        private readonly IQueueManager _queueManager;
        private readonly INmsMessageFactory _messageFactory;
        private readonly ILogger _logger;
        private ISession _session;
        private IMessageConsumer _consumer;

        public AMQPQueueReceiver(string queuePath,
                                     IQueueManager queueManager,
                                     INmsMessageFactory messageFactory,
                                     ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                     IGlobalHandlerThrottle globalHandlerThrottle,
                                     ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queuePath = queuePath;
            _queueManager = queueManager;
            _messageFactory = messageFactory;
            _logger = logger;
        }

        protected override async Task WarmUp()
        {
            _logger.Debug("Warming up queue receiver for {QueuePath}", _queuePath);

            _session = await _queueManager.CreateSession(AcknowledgementMode.ClientAcknowledge);
            var queue = await _queueManager.GetQueue(_session, _queuePath);
            _consumer = await _session.CreateConsumerAsync(queue);

            _logger.Info("Queue receiver for {QueuePath} is ready", _queuePath);
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

                _logger.Debug("Received message {MessageId} from queue {QueuePath}", nmsMessage.NMSMessageId, _queuePath);

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
                _logger.Error(ex, "Error fetching message from queue {QueuePath}", _queuePath);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger.Debug("Disposing queue receiver for {QueuePath}", _queuePath);

                try
                {
                    _consumer?.Close();
                    _consumer?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Error disposing consumer for {QueuePath}", _queuePath);
                }

                try
                {
                    _session?.Close();
                    _session?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Error disposing session for {QueuePath}", _queuePath);
                }
            }

            base.Dispose(disposing);
        }
    }
}
