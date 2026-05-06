using System;
using System.Threading;
using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AMQP.MessageConversion;
using Nimbus.Transports.AMQP.QueueManagement;

namespace Nimbus.Transports.AMQP.MessageSendersAndReceivers
{
    internal class AMQPQueueSender : INimbusMessageSender, IDisposable
    {
        private readonly string _queuePath;
        private readonly IQueueManager _queueManager;
        private readonly INmsMessageFactory _messageFactory;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private ISession _session;
        private IMessageProducer _producer;

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
            await _lock.WaitAsync();
            try
            {
                await EnsureProducer();
                var nmsMessage = await _messageFactory.CreateNmsMessage(message, _session);

                _logger.Debug("Sending message {MessageId} to queue {QueuePath}", message.MessageId, _queuePath);
                await _producer.SendAsync(nmsMessage).ConfigureAwaitFalse();
                _logger.Debug("Message {MessageId} sent successfully to queue {QueuePath}", message.MessageId, _queuePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send message {MessageId} to queue {QueuePath}", message.MessageId, _queuePath);
                ResetProducer();
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task EnsureProducer()
        {
            if (_producer != null) return;
            _session = await _queueManager.CreateSession(AcknowledgementMode.AutoAcknowledge);
            var queue = await _queueManager.GetQueue(_session, _queuePath);
            _producer = _session.CreateProducer(queue);
        }

        private void ResetProducer()
        {
            try { _producer?.Close(); } catch { /* ignore */ }
            try { _producer?.Dispose(); } catch { /* ignore */ }
            try { _session?.Close(); } catch { /* ignore */ }
            try { _session?.Dispose(); } catch { /* ignore */ }
            _producer = null;
            _session = null;
        }

        public void Dispose()
        {
            ResetProducer();
            _lock.Dispose();
        }
    }
}
