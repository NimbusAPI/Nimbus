namespace Nimbus.Transports.AzureServiceBus2.SendersAndRecievers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Nimbus.Extensions;
    using Nimbus.Infrastructure.MessageSendersAndReceivers;
    using Nimbus.Infrastructure.Retries;
    using Nimbus.InfrastructureContracts;
    using Nimbus.Transports.AzureServiceBus2.BrokeredMessages;
    using Nimbus.Transports.AzureServiceBus2.QueueManagement;

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
            this._brokeredMessageFactory = brokeredMessageFactory;
            this._retry = retry;
            this._queuePath = queuePath;
            this._logger = logger;
            this._queueManager = queueManager;
        }

        public async Task Send(NimbusMessage message)
        {
            await this._retry.DoAsync(async () =>
                                       {
                                           var brokeredMessage = await this._brokeredMessageFactory.BuildMessage(message);
                                           

                                           var messageSender = this.GetMessageSender();
                                           try
                                           {
                                               await messageSender.SendAsync(brokeredMessage);
                                           }
                                           catch (MessagingEntityNotFoundException exc)
                                           {
                                               this._logger.Error(exc, "The referenced queue {QueuePath} no longer exists", this._queuePath);
                                               await this._queueManager.MarkQueueAsNonExistent(this._queuePath);
                                               await this.DiscardMessageSender();
                                               throw;
                                           }
                                           catch (Exception)
                                           {
                                               await this.DiscardMessageSender();
                                               throw;
                                           }
                                       },
                                 "Sending message to queue").ConfigureAwaitFalse();
        }

        private IMessageSender GetMessageSender()
        {
            if (this._messageSender != null) return this._messageSender;

            this._messageSender = this._queueManager.CreateMessageSender(this._queuePath).Result;
            return this._messageSender;
        }

        private async Task DiscardMessageSender()    
        {
            var messageSender = this._messageSender;
            this._messageSender = null;

            if (messageSender == null) return;
            if (messageSender.IsClosedOrClosing) return;

            try
            {
                this._logger.Debug("Discarding message sender for {QueuePath}", this._queuePath);
                await messageSender.CloseAsync();
            }
            catch (Exception exc)
            {
                this._logger.Error(exc, "Failed to close MessageSender instance before discarding it.");
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.DiscardMessageSender();
        }
    }
}