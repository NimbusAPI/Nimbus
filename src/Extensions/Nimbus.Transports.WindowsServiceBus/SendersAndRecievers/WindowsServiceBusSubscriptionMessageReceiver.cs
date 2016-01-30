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
    internal class WindowsServiceBusSubscriptionMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private SubscriptionClient _subscriptionClient;

        public WindowsServiceBusSubscriptionMessageReceiver(IQueueManager queueManager,
                                                            string topicPath,
                                                            string subscriptionName,
                                                            ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                                            IBrokeredMessageFactory brokeredMessageFactory,
                                                            IGlobalHandlerThrottle globalHandlerThrottle,
                                                            ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _subscriptionName = subscriptionName;
            _brokeredMessageFactory = brokeredMessageFactory;
            _logger = logger;
        }

        public override string ToString()
        {
            return "{0}/{1}".FormatWith(_topicPath, _subscriptionName);
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

                    return nimbusMessage;
                }
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

            _subscriptionClient = await _queueManager.CreateSubscriptionReceiver(_topicPath, _subscriptionName);
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