using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Retries;
using Nimbus.Transports.AzureServiceBus.Messages;
using Nimbus.Transports.AzureServiceBus.QueueManagement;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AzureServiceBus.SendersAndRecievers
{
    internal class AzureServiceBusTopicMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IMessageFactory _MessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly ILogger _logger;

        private TopicClient _topicClient;
        private readonly IRetry _retry;

        public AzureServiceBusTopicMessageSender(IMessageFactory MessageFactory, ILogger logger, IQueueManager queueManager, IRetry retry, string topicPath)
        {
            _queueManager = queueManager;
            _retry = retry;
            _topicPath = topicPath;
            _logger = logger;
            _MessageFactory = MessageFactory;
        }

        public async Task Send(NimbusMessage message)
        {
            await _retry.DoAsync(async () =>
                                       {
                                           var Message = await _MessageFactory.BuildMessage(message);

                                           var topicClient = GetTopicClient();
                                           try
                                           {
                                               await topicClient.SendAsync(Message);
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

        private TopicClient GetTopicClient()
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
            if (topicClient.IsClosed) return;

            try
            {
                _logger.Debug("Discarding message sender for {TopicPath}", _topicPath);
                topicClient.Close();
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