using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Retries;
using Nimbus.Transports.AzureServiceBus.Messages;
using Nimbus.Transports.AzureServiceBus.QueueManagement;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AzureServiceBus.SendersAndRecievers
{
    internal class AzureServiceBusQueueMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IMessageFactory _messageFactory;
        private readonly IQueueManager _queueManager;
        private readonly ILogger _logger;
        private readonly IRetry _retry;
        private readonly string _queuePath;

        private MessageSender _messageSender;

        public AzureServiceBusQueueMessageSender(IMessageFactory messageFactory, ILogger logger, IQueueManager queueManager, IRetry retry, string queuePath)
        {
            _messageFactory = messageFactory;
            _queueManager = queueManager;
            _retry = retry;
            _queuePath = queuePath;
            _logger = logger;
        }

        public async Task Send(NimbusMessage message)
        {
            await _retry.DoAsync(async () =>
                                       {
                                           var Message = await _messageFactory.BuildMessage(message);

                                           var messageSender = GetMessageSender();
                                           try
                                           {
                                               await messageSender.SendAsync(Message);
                                           }
                                           catch (MessagingEntityNotFoundException exc)
                                           {
                                               _logger.Error(exc, "The referenced queue {QueuePath} no longer exists", _queuePath);
                                               await _queueManager.MarkQueueAsNonExistent(_queuePath);
                                               await DiscardMessageSender();
                                               throw;
                                           }
                                           catch (Exception)
                                           {
                                               await DiscardMessageSender();
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

        private async Task DiscardMessageSender()
        {
            var messageSender = _messageSender;
            _messageSender = null;

            if (messageSender == null) return;
            if (messageSender.IsClosedOrClosing) return;

            try
            {
                _logger.Debug("Discarding message sender for {QueuePath}", _queuePath);
                await messageSender.CloseAsync();
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