using System;
using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Routing;
using Nimbus.Transports.AMQP.MessageConversion;
using Nimbus.Transports.AMQP.QueueManagement;

namespace Nimbus.Transports.AMQP.DeadLetter
{
    internal class AMQPDeadLetterOffice : IDeadLetterOffice
    {
        private readonly IQueueManager _queueManager;
        private readonly INmsMessageFactory _messageFactory;
        private readonly string _deadLetterQueuePath;
        private readonly ILogger _logger;

        public AMQPDeadLetterOffice(IQueueManager queueManager,
                                    INmsMessageFactory messageFactory,
                                    IPathFactory pathFactory,
                                    ILogger logger)
        {
            _queueManager = queueManager;
            _messageFactory = messageFactory;
            _deadLetterQueuePath = pathFactory.DeadLetterOfficePath();
            _logger = logger;
        }

        public async Task<NimbusMessage> Peek()
        {
            using var session = await _queueManager.CreateSession(AcknowledgementMode.AutoAcknowledge);
            var queue = await _queueManager.GetQueue(session, _deadLetterQueuePath);
            using var browser = await session.CreateBrowserAsync(queue);

            var enumerator = browser.GetEnumerator();
            if (!enumerator.MoveNext()) return null;
            
            var nmsMessage = (IMessage) enumerator.Current;
            return await _messageFactory.CreateNimbusMessage(nmsMessage);

        }

        public async Task<NimbusMessage> Pop()
        {
            using var session = await _queueManager.CreateSession(AcknowledgementMode.ClientAcknowledge);
            var queue = await _queueManager.GetQueue(session, _deadLetterQueuePath);
            using var consumer = await session.CreateConsumerAsync(queue);

            var nmsMessage = await Task.Run(() => consumer.Receive(TimeSpan.FromMilliseconds(500)))
                .ConfigureAwaitFalse();

            if (nmsMessage == null) return null;

            var nimbusMessage = await _messageFactory.CreateNimbusMessage(nmsMessage);
            await nmsMessage.AcknowledgeAsync();

            return nimbusMessage;
        }

        public async Task Post(NimbusMessage message)
        {
            _logger.Warn("Sending message {MessageId} to dead letter queue {DeadLetterQueue}",
                         message.MessageId, _deadLetterQueuePath);

            try
            {
                using var session = await _queueManager.CreateSession(AcknowledgementMode.AutoAcknowledge);
                var queue = await _queueManager.GetQueue(session, _deadLetterQueuePath);
                using var producer = session.CreateProducer(queue);

                var nmsMessage = await _messageFactory.CreateNmsMessage(message, session);

                await producer.SendAsync(nmsMessage).ConfigureAwaitFalse();
                _logger.Info("Message {MessageId} successfully moved to dead letter queue", message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send message {MessageId} to dead letter queue", message.MessageId);
                throw;
            }
        }

        public async Task<int> Count()
        {
            using var session = await _queueManager.CreateSession(AcknowledgementMode.AutoAcknowledge);
            var queue = await _queueManager.GetQueue(session, _deadLetterQueuePath);
            using var browser = await session.CreateBrowserAsync(queue);

            var enumerator = browser.GetEnumerator();
            var count = 0;
            while (enumerator.MoveNext())
            {
                count++;
            }

            return count;
        }
    }
}
