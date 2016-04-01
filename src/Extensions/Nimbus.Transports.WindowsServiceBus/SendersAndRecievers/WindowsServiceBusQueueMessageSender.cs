using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Retries;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;
using Nimbus.Transports.WindowsServiceBus.QueueManagement;

namespace Nimbus.Transports.WindowsServiceBus.SendersAndRecievers
{
    internal class WindowsServiceBusQueueMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly ILogger _logger;
        private readonly IRetry _retry;
        private readonly string _queuePath;

        private MessageSender _messageSender;

        public WindowsServiceBusQueueMessageSender(IBrokeredMessageFactory brokeredMessageFactory, ILogger logger, IQueueManager queueManager, IRetry retry, string queuePath)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _queueManager = queueManager;
            _retry = retry;
            _queuePath = queuePath;
            _logger = logger;
        }

        public async Task Send(NimbusMessage message)
        {
            await _retry.DoAsync(async () =>
                                       {
                                           var brokeredMessage = await _brokeredMessageFactory.BuildBrokeredMessage(message);

                                           var messageSender = GetMessageSender();
                                           try
                                           {
                                               await messageSender.SendAsync(brokeredMessage);
                                           }
                                           catch (MessagingEntityNotFoundException exc)
                                           {
                                               _logger.Error(exc, "The referenced queue {QueuePath} no longer exists", _queuePath);
                                               await _queueManager.MarkQueueAsNonExistent(_queuePath);
                                               DiscardMessageSender();
                                               throw;
                                           }
                                           catch (Exception)
                                           {
                                               DiscardMessageSender();
                                               throw;
                                           }
                                       },
                                 "Sending message to queue").ConfigureAwaitFalse();
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
                _logger.Debug("Discarding message sender for {QueuePath}", _queuePath);
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