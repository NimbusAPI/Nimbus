using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
using Nimbus.Transports.AzureServiceBus.QueueManagement;

namespace Nimbus.Transports.AzureServiceBus.SendersAndRecievers
{
    internal class AzureServiceBusQueueMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;
        private readonly RequireRetriesToBeHandledBy _requireRetriesToBeHandledBy;
        private readonly ILogger _logger;

        private volatile MessageReceiver _messageReceiver;
        private readonly ThreadSafeDictionary<Guid, BrokeredMessage> _trackedMessages = new ThreadSafeDictionary<Guid, BrokeredMessage>();

        public AzureServiceBusQueueMessageReceiver(IBrokeredMessageFactory brokeredMessageFactory,
                                                   IQueueManager queueManager,
                                                   string queuePath,
                                                   ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                                   RequireRetriesToBeHandledBy requireRetriesToBeHandledBy,
                                                   IGlobalHandlerThrottle globalHandlerThrottle,
                                                   ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
            _requireRetriesToBeHandledBy = requireRetriesToBeHandledBy;
            _logger = logger;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public override string ToString()
        {
            return _queuePath;
        }

        public override async Task RecordSuccess(NimbusMessage message)
        {
            if (_requireRetriesToBeHandledBy == RetriesHandledBy.Bus) return;

            try
            {
                BrokeredMessage brokeredMessage;
                if (!_trackedMessages.TryRemove(message.MessageId, out brokeredMessage)) return;
                await brokeredMessage.CompleteAsync();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Failed to complete {MessageId}", message.MessageId);
            }
        }

        public override async Task RecordFailure(NimbusMessage message)
        {
            if (_requireRetriesToBeHandledBy == RetriesHandledBy.Bus) return;

            try
            {
                BrokeredMessage brokeredMessage;
                if (!_trackedMessages.TryRemove(message.MessageId, out brokeredMessage)) return;
                await brokeredMessage.AbandonAsync();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Failed to abandon {MessageId}", message.MessageId);
            }
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