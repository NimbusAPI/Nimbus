namespace Nimbus.Transports.AzureServiceBus2.SendersAndRecievers
{
    using System;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using Nimbus.Extensions;
    using Nimbus.Infrastructure.MessageSendersAndReceivers;
    using Nimbus.Infrastructure.Retries;
    using Nimbus.InfrastructureContracts;
    using Nimbus.Transports.AzureServiceBus2.BrokeredMessages;
    using Nimbus.Transports.AzureServiceBus2.QueueManagement;

    internal class AzureServiceBusTopicMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly ILogger _logger;

        private ServiceBusSender _topicClient;
        private readonly IRetry _retry;

        public AzureServiceBusTopicMessageSender(IBrokeredMessageFactory brokeredMessageFactory, ILogger logger, IQueueManager queueManager, IRetry retry, string topicPath)
        {
            this._queueManager = queueManager;
            this._retry = retry;
            this._topicPath = topicPath;
            this._logger = logger;
            this._brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task Send(NimbusMessage nimbusMessage)
        {
            await this._retry.DoAsync(async () =>
                                       {
                                           var message = await this._brokeredMessageFactory.BuildMessage(nimbusMessage);

                                           var topicClient = this.GetTopicClient();
                                           try
                                           {
                                               await topicClient.SendMessageAsync(message);
                                           }
                                           catch (ServiceBusException exc) when(exc.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
                                           {
                                               this._logger.Error(exc, "The referenced topic path {TopicPath} no longer exists", this._topicPath);
                                               await this._queueManager.MarkTopicAsNonExistent(this._topicPath);
                                               this.DiscardTopicClient();
                                               throw;
                                           }
                                           catch (Exception)
                                           {
                                               this.DiscardTopicClient();
                                               throw;
                                           }
                                       },
                                 "Sending message to topic").ConfigureAwaitFalse();
        }

        private ServiceBusSender GetTopicClient()
        {
            if (this._topicClient != null) return this._topicClient;

            this._topicClient = this._queueManager.CreateTopicSender(this._topicPath).Result;
            return this._topicClient;
        }

        private void DiscardTopicClient()
        {
            var topicClient = this._topicClient;
            this._topicClient = null;

            if (topicClient == null) return;
            if (topicClient.IsClosed) return;

            try
            {
                this._logger.Debug("Discarding message sender for {TopicPath}", this._topicPath);
                topicClient.CloseAsync();
            }
            catch (Exception exc)
            {
                this._logger.Error(exc, "Failed to close TopicClient instance before discarding it.");
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.DiscardTopicClient();
        }
    }
}