using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
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
            _queueManager = queueManager;
            _topicPath = topicPath;
            _subscriptionName = subscriptionName;
            _filterCondition = filterCondition;
            _brokeredMessageFactory = brokeredMessageFactory;
            _logger = logger;
        }

        public override string ToString()
        {
            return "{0}/{1}".FormatWith(_topicPath, _subscriptionName);
        }

        protected override async Task WarmUp()
        {
            var subscriptionClient = await GetSubscriptionClient();
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                                        {
                                            // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                                            // Set it according to how many messages the application wants to process in parallel.
                                            MaxConcurrentCalls = 1,

                                            // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                                            // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                                            AutoComplete = false
                                        };
            subscriptionClient.RegisterMessageHandler(OnMessageRecieved, messageHandlerOptions);
            
            _logger.Debug("Client warmed up");
        }

        private async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
        {
            switch (args.Exception)
            {
                
                case MessagingEntityNotFoundException ex:
                    _logger.Warn(ex.Message);
                    return;
                case ServiceBusException sbe when sbe.IsTransient:
                    _logger.Warn(sbe.Message);
                    return;
            }

            _logger.Error(args.Exception, "Messaging operation failed. Discarding message receiver.");
            DiscardSubscriptionClient();
            throw args.Exception;
        }

        private Task OnMessageRecieved(Message message, CancellationToken cancellationToken)
        {
            _messages.Add(message, cancellationToken);
            _receiveSemaphore.Release();
            return Task.CompletedTask;
        }

        protected override Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
                            {
                                await _receiveSemaphore.WaitAsync(_pollInterval, cancellationToken);

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    await _subscriptionClient.CloseAsync();
                                    return null;
                                }

                                if (_messages.Count == 0)
                                    return null;

                                var message = _messages.Take();

                                var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(message);
                                nimbusMessage.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscriptionName;

                                return nimbusMessage;
                            },
                            cancellationToken).ConfigureAwaitFalse();
            
        }

        private async Task<ISubscriptionClient> GetSubscriptionClient()
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
            if (subscriptionClient.IsClosedOrClosing) return;
            try
            {
                
                _logger.Debug($"Closing client for {_subscriptionName}");
                subscriptionClient.CloseAsync();
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "An exception occurred while closing a SubscriptionClient.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            _logger.Debug($"Disposing client for {_subscriptionName}");
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