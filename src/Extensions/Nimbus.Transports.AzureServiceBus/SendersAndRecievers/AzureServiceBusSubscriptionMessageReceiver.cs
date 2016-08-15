using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Filtering.Attributes;
using Nimbus.Filtering.Conditions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
using Nimbus.Transports.AzureServiceBus.Filtering;
using Nimbus.Transports.AzureServiceBus.QueueManagement;

namespace Nimbus.Transports.AzureServiceBus.SendersAndRecievers
{
    internal class AzureServiceBusSubscriptionMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private readonly IFilterCondition _filterCondition;
        private readonly RequireRetriesToBeHandledBy _requireRetriesToBeHandledBy;
        private SubscriptionClient _subscriptionClient;

        private readonly ThreadSafeDictionary<Guid, BrokeredMessage> _trackedMessages = new ThreadSafeDictionary<Guid, BrokeredMessage>();

        public AzureServiceBusSubscriptionMessageReceiver(IQueueManager queueManager,
                                                          string topicPath,
                                                          string subscriptionName,
                                                          IFilterCondition filterCondition,
                                                          ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                                          RequireRetriesToBeHandledBy requireRetriesToBeHandledBy,
                                                          IBrokeredMessageFactory brokeredMessageFactory,
                                                          IGlobalHandlerThrottle globalHandlerThrottle,
                                                          ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _subscriptionName = subscriptionName;
            _filterCondition = filterCondition;
            _requireRetriesToBeHandledBy = requireRetriesToBeHandledBy;
            _brokeredMessageFactory = brokeredMessageFactory;
            _logger = logger;
        }

        public override string ToString()
        {
            return "{0}/{1}".FormatWith(_topicPath, _subscriptionName);
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
            await GetSubscriptionClient();
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
                    var subscriptionClient = await GetSubscriptionClient();

                    var receiveTask = subscriptionClient.ReceiveAsync(TimeSpan.FromSeconds(300)).ConfigureAwaitFalse();
                    var cancellationTask = Task.Run(async () => await CancellationTask(cancellationSemaphore, cancellationToken), cancellationToken).ConfigureAwaitFalse();
                    await Task.WhenAny(receiveTask, cancellationTask);
                    if (!receiveTask.IsCompleted) return null;

                    cancellationSemaphore.Release();

                    var brokeredMessage = await receiveTask;
                    if (brokeredMessage == null) return null;

                    var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(brokeredMessage);
                    nimbusMessage.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscriptionName;

                    _trackedMessages.TryAdd(nimbusMessage.MessageId, brokeredMessage);

                    return nimbusMessage;
                }
            }
            catch (MessagingEntityNotFoundException exc)
            {
                _logger.Error(exc, "The referenced topic subscription {TopicPath}/{SubscriptionName} no longer exists", _topicPath, _subscriptionName);
                await _queueManager.MarkSubscriptionAsNonExistent(_topicPath, _subscriptionName);
                DiscardSubscriptionClient();
                throw;
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Messaging operation failed. Discarding message receiver.");
                DiscardSubscriptionClient();
                throw;
            }
        }

        private async Task<SubscriptionClient> GetSubscriptionClient()
        {
            if (_subscriptionClient != null) return _subscriptionClient;

            _subscriptionClient = await _queueManager.CreateSubscriptionReceiver(_topicPath, _subscriptionName, _filterCondition);
            _subscriptionClient.PrefetchCount = ConcurrentHandlerLimit;
            return _subscriptionClient;
        }

        private void DiscardSubscriptionClient()
        {
            var subscriptionClient = _subscriptionClient;
            _subscriptionClient = null;

            if (subscriptionClient == null) return;
            if (subscriptionClient.IsClosed) return;

            try
            {
                subscriptionClient.Close();
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "An exception occurred while closing a SubscriptionClient.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing) return;

                DiscardSubscriptionClient();
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