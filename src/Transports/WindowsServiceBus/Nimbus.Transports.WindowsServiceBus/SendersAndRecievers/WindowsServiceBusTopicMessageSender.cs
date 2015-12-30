using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;

namespace Nimbus.Transports.WindowsServiceBus.SendersAndRecievers
{
    internal class WindowsServiceBusTopicMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly ILogger _logger;

        private TopicClient _topicClient;
        private readonly Retry _retry;

        public WindowsServiceBusTopicMessageSender(IBrokeredMessageFactory brokeredMessageFactory, IQueueManager queueManager, string topicPath, ILogger logger)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _logger = logger;
            _brokeredMessageFactory = brokeredMessageFactory;
            _retry = new Retry(5)
                .Chain(r => r.Started += (s, e) => _logger.Debug("{Action}...", e.ActionName))
                .Chain(r => r.Success += (s, e) => _logger.Debug("{Action} completed successfully in {Elapsed}.", e.ActionName, e.ElapsedTime))
                .Chain(r => r.TransientFailure += (s, e) => _logger.Warn(e.Exception, "A transient failure occurred in action {Action}.", e.ActionName))
                .Chain(r => r.PermanentFailure += (s, e) => _logger.Error(e.Exception, "A permanent failure occurred in action {Action}.", e.ActionName));
        }

        public async Task Send(NimbusMessage message)
        {
            await _retry.DoAsync(async () =>
                                       {
                                           var brokeredMessage = await _brokeredMessageFactory.BuildBrokeredMessage(message);

                                           var topicClient = GetTopicClient();
                                           try
                                           {
                                               await topicClient.SendAsync(brokeredMessage);
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
                _logger.Info("Discarding message sender for {TopicPath}", _topicPath);
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