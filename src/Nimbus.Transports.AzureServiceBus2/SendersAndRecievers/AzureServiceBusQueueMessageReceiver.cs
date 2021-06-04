namespace Nimbus.Transports.AzureServiceBus2.SendersAndRecievers
{
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
    using Nimbus.Transports.AzureServiceBus2.BrokeredMessages;
    using Nimbus.Transports.AzureServiceBus2.QueueManagement;

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
            this._queueManager = queueManager;
            this._queuePath = queuePath;
            this._logger = logger;
            this._brokeredMessageFactory = brokeredMessageFactory;
        }

        public override string ToString()
        {
            return this._queuePath;
        }

        protected override async Task WarmUp()
        {
            await this.GetMessageReceiver();
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
                    var messageReceiver = await this.GetMessageReceiver();
                    var receiveTask = messageReceiver.ReceiveAsync(TimeSpan.FromSeconds(300)).ConfigureAwaitFalse();
                    var cancellationTask = Task.Run(async () => await this.CancellationTask(cancellationSemaphore, cancellationToken), cancellationToken).ConfigureAwaitFalse();

                    await Task.WhenAny(receiveTask, cancellationTask);
                    if (!receiveTask.IsCompleted) return null;

                    cancellationSemaphore.Release();

                    var message = await receiveTask;
                    if (message == null) return null;

                    var nimbusMessage = await this._brokeredMessageFactory.BuildNimbusMessage(message);
                    return nimbusMessage;
                }
            }
            catch (MessagingEntityNotFoundException exc)
            {
                this._logger.Error(exc, "The referenced queue {QueuePath} no longer exists", this._queuePath);
                await this._queueManager.MarkQueueAsNonExistent(this._queuePath);
                this.DiscardMessageReceiver();
                throw;
            }
            catch (Exception exc)
            {
                this._logger.Error(exc, "Messaging operation failed. Discarding message receiver.");
                this.DiscardMessageReceiver();
                throw;
            }
        }

        private async Task<IMessageReceiver> GetMessageReceiver()
        {
            if (this._messageReceiver != null) return this._messageReceiver;

            this._messageReceiver = await this._queueManager.CreateMessageReceiver(this._queuePath);
            this._messageReceiver.PrefetchCount = this.ConcurrentHandlerLimit;
            return this._messageReceiver;
        }

        private async Task DiscardMessageReceiver()
        {
            var messageReceiver = this._messageReceiver;
            this._messageReceiver = null;

            if (messageReceiver == null) return;
            //TODO
            if (messageReceiver.IsClosedOrClosing) return;

            try
            {
                await messageReceiver.CloseAsync();
            }
            catch (Exception exc)
            {
                this._logger.Error(exc, "An exception occurred while closing a MessageReceiver.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing) return;

                this.DiscardMessageReceiver();
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