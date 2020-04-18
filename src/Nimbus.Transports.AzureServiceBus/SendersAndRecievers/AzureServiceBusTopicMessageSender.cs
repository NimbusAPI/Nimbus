using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Retries;
using Nimbus.Transports.AzureServiceBus.QueueManagement;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;

namespace Nimbus.Transports.AzureServiceBus.SendersAndRecievers
{
    internal class AzureServiceBusTopicMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly ILogger _logger;

        private ITopicClient _topicClient;
        private readonly IRetry _retry;

        public AzureServiceBusTopicMessageSender(IBrokeredMessageFactory brokeredMessageFactory, ILogger logger, IQueueManager queueManager, IRetry retry, string topicPath)
        {
            _queueManager = queueManager;
            _retry = retry;
            _topicPath = topicPath;
            _logger = logger;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task Send(NimbusMessage nimbusMessage)
        {
            await _retry.DoAsync(async () =>
                                       {
                                           var message = await _brokeredMessageFactory.BuildMessage(nimbusMessage);

                                           var topicClient = GetTopicClient();
                                           try
                                           {
                                               await topicClient.SendAsync(message);
                                           }
                                           catch (MessagingEntityNotFoundException exc)
                                           {
                                               _logger.Error(exc, "The referenced topic path {TopicPath} no longer exists", _topicPath);
                                               await _queueManager.MarkTopicAsNonExistent(_topicPath);
                                               DiscardTopicClient();
                                               throw;
                                           }
                                           catch (Exception)
                                           {
                                               DiscardTopicClient();
                                               throw;
                                           }
                                       },
                                 "Sending message to topic").ConfigureAwaitFalse();
        }

        private ITopicClient GetTopicClient()
        {
            if (_topicClient != null) return _topicClient;

            _topicClient = _queueManager.CreateTopicSender(_topicPath).Result;
            return _topicClient;
        }

        private void DiscardTopicClient()
        {
            var topicClient = _topicClient;
            _topicClient = null;

            if (topicClient == null) return;
            if (topicClient.IsClosedOrClosing) return;

            try
            {
                _logger.Debug("Discarding message sender for {TopicPath}", _topicPath);
                topicClient.CloseAsync();
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Failed to close TopicClient instance before discarding it.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            DiscardTopicClient();
        }
    }
}