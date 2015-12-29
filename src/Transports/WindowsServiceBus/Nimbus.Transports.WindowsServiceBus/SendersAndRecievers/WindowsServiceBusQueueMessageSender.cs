using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;

namespace Nimbus.Transports.WindowsServiceBus.SendersAndRecievers
{
    internal class WindowsServiceBusQueueMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;
        private readonly ILogger _logger;

        private MessageSender _messageSender;
        private const int _maxRetries = 5;

        public WindowsServiceBusQueueMessageSender(IBrokeredMessageFactory brokeredMessageFactory, IQueueManager queueManager, string queuePath, ILogger logger)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _queueManager = queueManager;
            _queuePath = queuePath;
            _logger = logger;
        }

        public async Task Send(NimbusMessage message)
        {
            var attempts = 0;
            while (true)
            {
                attempts++;
                var brokeredMessage = await _brokeredMessageFactory.BuildBrokeredMessage(message);

                var messageSender = GetMessageSender();
                try
                {
                    await messageSender.SendAsync(brokeredMessage);
                    break;
                }
                catch (Exception)
                {
                    DiscardMessageSender();
                    if (attempts > _maxRetries) throw;
                }
            }
        }

        private MessageSender GetMessageSender()
        {
            if (_messageSender != null) return _messageSender;

            _messageSender = _queueManager.CreateMessageSender(_queuePath).Result;
            return _messageSender;
        }

        private void DiscardMessageSender()
        {
            var messageSender = _messageSender;
            _messageSender = null;

            if (messageSender == null) return;
            if (messageSender.IsClosed) return;

            try
            {
                _logger.Info("Discarding message sender for {QueuePath}", _queuePath);
                messageSender.Close();
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Failed to close MessageSender instance before discarding it.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            DiscardMessageSender();
        }
    }
}