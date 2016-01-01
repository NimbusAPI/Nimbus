using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;
using Nimbus.Transports.WindowsServiceBus.QueueManagement;

namespace Nimbus.Transports.WindowsServiceBus.SendersAndRecievers
{
    internal class WindowsServiceBusQueueMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;
        private readonly ILogger _logger;

        private MessageSender _messageSender;
        private readonly Retry _retry;

        public WindowsServiceBusQueueMessageSender(IBrokeredMessageFactory brokeredMessageFactory, IQueueManager queueManager, string queuePath, ILogger logger)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _queueManager = queueManager;
            _queuePath = queuePath;
            _logger = logger;

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

                                           var messageSender = GetMessageSender();
                                           try
                                           {
                                               await messageSender.SendAsync(brokeredMessage);
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