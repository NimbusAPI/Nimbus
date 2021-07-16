using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Retries;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
using Nimbus.Transports.AzureServiceBus.QueueManagement;

namespace Nimbus.Transports.AzureServiceBus.SendersAndReceivers
{
    internal class AzureServiceBusQueueMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;
        private readonly IQueueManager _queueManager;
        private readonly IRetry _retry;
        private readonly string _queuePath;

        private IMessageSender _messageSender;

        public AzureServiceBusQueueMessageSender(IBrokeredMessageFactory brokeredMessageFactory, ILogger logger, IQueueManager queueManager, IRetry retry, string queuePath)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _retry = retry;
            _queuePath = queuePath;
            _logger = logger;
            _queueManager = queueManager;
        }

        public async Task Send(NimbusMessage message)
        {
            await _retry.DoAsync(async () =>
                                       {
                                           var brokeredMessage = await _brokeredMessageFactory.BuildMessage(message);
                                           

                                           var messageSender = GetMessageSender();
                                           try
                                           {
                                               await messageSender.SendAsync(brokeredMessage);
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

        private IMessageSender GetMessageSender()
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