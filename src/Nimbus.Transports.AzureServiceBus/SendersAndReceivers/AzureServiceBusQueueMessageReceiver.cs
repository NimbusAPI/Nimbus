using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
using Nimbus.Transports.AzureServiceBus.QueueManagement;

namespace Nimbus.Transports.AzureServiceBus.SendersAndReceivers
{
    internal class AzureServiceBusQueueMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;
        private readonly ILogger _logger;

        private volatile IMessageReceiver _messageReceiver;

        public AzureServiceBusQueueMessageReceiver(IBrokeredMessageFactory brokeredMessageFactory,
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

                    var message = await receiveTask;
                    if (message == null) return null;

                    var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(message);
                    return nimbusMessage;
                }
            }
            catch (MessagingEntityNotFoundException exc)
            {
                _logger.Error(exc, "The referenced queue {QueuePath} no longer exists", _queuePath);
                await _queueManager.MarkQueueAsNonExistent(_queuePath);
                DiscardMessageReceiver();
                throw;
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Messaging operation failed. Discarding message receiver.");
                DiscardMessageReceiver();
                throw;
            }
        }

        private async Task<IMessageReceiver> GetMessageReceiver()
        {
            if (_messageReceiver != null) return _messageReceiver;

            _messageReceiver = await _queueManager.CreateMessageReceiver(_queuePath);
            _messageReceiver.PrefetchCount = ConcurrentHandlerLimit;
            return _messageReceiver;
        }

        private async Task DiscardMessageReceiver()
        {
            var messageReceiver = _messageReceiver;
            _messageReceiver = null;

            if (messageReceiver == null) return;
            //TODO
            if (messageReceiver.IsClosedOrClosing) return;

            try
            {
                await messageReceiver.CloseAsync();
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