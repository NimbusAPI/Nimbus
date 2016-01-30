using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;
using Nimbus.Transports.WindowsServiceBus.QueueManagement;

namespace Nimbus.Transports.WindowsServiceBus.SendersAndRecievers
{
    internal class WindowsServiceBusQueueMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;
        private readonly ILogger _logger;

        private volatile MessageReceiver _messageReceiver;

        public WindowsServiceBusQueueMessageReceiver(IBrokeredMessageFactory brokeredMessageFactory,
                                                     IQueueManager queueManager,
                                                     string queuePath,
                                                     ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                                     IGlobalHandlerThrottle globalHandlerThrottle,
                                                     ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
            _logger = logger;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public override string ToString()
        {
            return _queuePath;
        }

        protected override async Task WarmUp()
        {
            await GetMessageReceiver();
        }

        private async Task CancellationTask(SemaphoreSlim cancellationSemaphore, CancellationToken cancellationToken)
        {
            try
            {
                await cancellationSemaphore.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            try
            {
                using (var cancellationSemaphore = new SemaphoreSlim(0, int.MaxValue))
                {
                    var messageReceiver = await GetMessageReceiver();
                    var receiveTask = messageReceiver.ReceiveAsync(TimeSpan.FromSeconds(300)).ConfigureAwaitFalse();
                    var cancellationTask = Task.Run(async () => await CancellationTask(cancellationSemaphore, cancellationToken), cancellationToken).ConfigureAwaitFalse();

                    await Task.WhenAny(receiveTask, cancellationTask);
                    if (!receiveTask.IsCompleted) return null;

                    cancellationSemaphore.Release();

                    var brokeredMessage = await receiveTask;
                    if (brokeredMessage == null) return null;

                    var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(brokeredMessage);
                    return nimbusMessage;
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Messaging operation failed. Discarding message receiver.");
                DiscardMessageReceiver();
                throw;
            }
        }

        private async Task<MessageReceiver> GetMessageReceiver()
        {
            if (_messageReceiver != null) return _messageReceiver;

            _messageReceiver = await _queueManager.CreateMessageReceiver(_queuePath);
            _messageReceiver.PrefetchCount = ConcurrentHandlerLimit;
            return _messageReceiver;
        }

        private void DiscardMessageReceiver()
        {
            var messageReceiver = _messageReceiver;
            _messageReceiver = null;

            if (messageReceiver == null) return;
            if (messageReceiver.IsClosed) return;

            try
            {
                messageReceiver.Close();
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "An exception occurred while closing a MessageReceiver.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing) return;

                DiscardMessageReceiver();
            }
            catch (MessagingEntityNotFoundException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}