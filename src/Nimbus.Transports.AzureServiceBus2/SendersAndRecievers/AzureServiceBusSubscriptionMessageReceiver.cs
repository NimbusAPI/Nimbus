namespace Nimbus.Transports.AzureServiceBus2.SendersAndRecievers
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Nimbus.Configuration.Settings;
    using Nimbus.Extensions;
    using Nimbus.Infrastructure;
    using Nimbus.Infrastructure.MessageSendersAndReceivers;
    using Nimbus.InfrastructureContracts;
    using Nimbus.InfrastructureContracts.Filtering.Conditions;
    using Nimbus.Transports.AzureServiceBus2.BrokeredMessages;
    using Nimbus.Transports.AzureServiceBus2.QueueManagement;

    internal class AzureServiceBusSubscriptionMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private readonly IFilterCondition _filterCondition;
        private ISubscriptionClient _subscriptionClient;
        private BlockingCollection<Message> _messages = new BlockingCollection<Message>();
        readonly SemaphoreSlim _receiveSemaphore = new SemaphoreSlim(0, int.MaxValue);
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

        public AzureServiceBusSubscriptionMessageReceiver(IQueueManager queueManager,
                                                          string topicPath,
                                                          string subscriptionName,
                                                          IFilterCondition filterCondition,
                                                          ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                                          IBrokeredMessageFactory brokeredMessageFactory,
                                                          IGlobalHandlerThrottle globalHandlerThrottle,
                                                          ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            this._queueManager = queueManager;
            this._topicPath = topicPath;
            this._subscriptionName = subscriptionName;
            this._filterCondition = filterCondition;
            this._brokeredMessageFactory = brokeredMessageFactory;
            this._logger = logger;
        }

        public override string ToString()
        {
            return "{0}/{1}".FormatWith(this._topicPath, this._subscriptionName);
        }

        protected override async Task WarmUp()
        {
            var subscriptionClient = await this.GetSubscriptionClient();
            var messageHandlerOptions = new MessageHandlerOptions(this.ExceptionReceivedHandler)
                                        {
                                            // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                                            // Set it according to how many messages the application wants to process in parallel.
                                            MaxConcurrentCalls = 1,

                                            // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                                            // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                                            AutoComplete = false
                                        };
            subscriptionClient.RegisterMessageHandler(this.OnMessageRecieved, messageHandlerOptions);
            
            this._logger.Debug("Client warmed up");
        }

        private async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
        {
            switch (args.Exception)
            {
                
                case MessagingEntityNotFoundException ex:
                    this._logger.Warn(ex.Message);
                    return;
                case ServiceBusException sbe when sbe.IsTransient:
                    this._logger.Warn(sbe.Message);
                    return;
            }

            this._logger.Error(args.Exception, "Messaging operation failed. Discarding message receiver.");
            this.DiscardSubscriptionClient();
            throw args.Exception;
        }

        private Task OnMessageRecieved(Message message, CancellationToken cancellationToken)
        {
            this._messages.Add(message, cancellationToken);
            this._receiveSemaphore.Release();
            return Task.CompletedTask;
        }

        protected override Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
                            {
                                await this._receiveSemaphore.WaitAsync(this._pollInterval, cancellationToken);

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    await this._subscriptionClient.CloseAsync();
                                    return null;
                                }

                                if (this._messages.Count == 0)
                                    return null;

                                var message = this._messages.Take();

                                var nimbusMessage = await this._brokeredMessageFactory.BuildNimbusMessage(message);
                                nimbusMessage.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = this._subscriptionName;

                                return nimbusMessage;
                            },
                            cancellationToken).ConfigureAwaitFalse();
            
        }

        private async Task<ISubscriptionClient> GetSubscriptionClient()
        {
            if (this._subscriptionClient != null) return this._subscriptionClient;

            this._subscriptionClient = await this._queueManager.CreateSubscriptionReceiver(this._topicPath, this._subscriptionName, this._filterCondition);
            this._subscriptionClient.PrefetchCount = this.ConcurrentHandlerLimit;
            
            return this._subscriptionClient;
        }

        private void DiscardSubscriptionClient()
        {
            var subscriptionClient = this._subscriptionClient;
            this._subscriptionClient = null;

            if (subscriptionClient == null) return;
            if (subscriptionClient.IsClosedOrClosing) return;
            try
            {
                
                this._logger.Debug($"Closing client for {this._subscriptionName}");
                subscriptionClient.CloseAsync();
            }
            catch (Exception exc)
            {
                this._logger.Error(exc, "An exception occurred while closing a SubscriptionClient.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            this._logger.Debug($"Disposing client for {this._subscriptionName}");
            try
            {
                if (!disposing) return;

                this.DiscardSubscriptionClient();
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