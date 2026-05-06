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
    internal class AMQPTopicSender : INimbusMessageSender, IDisposable
    {
        private readonly string _topicPath;
        private readonly IQueueManager _queueManager;
        private readonly INmsMessageFactory _messageFactory;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private ISession _session;
        private IMessageProducer _producer;

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
            await _lock.WaitAsync();
            try
            {
                await EnsureProducer();
                var nmsMessage = await _messageFactory.CreateNmsMessage(message, _session);

                _logger.Debug("Publishing message {MessageId} to topic {TopicPath}", message.MessageId, _topicPath);
                await _producer.SendAsync(nmsMessage).ConfigureAwaitFalse();
                _logger.Debug("Message {MessageId} published successfully to topic {TopicPath}", message.MessageId, _topicPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to publish message {MessageId} to topic {TopicPath}", message.MessageId, _topicPath);
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
            var topic = await _queueManager.GetTopic(_session, _topicPath);
            _producer = _session.CreateProducer(topic);
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
