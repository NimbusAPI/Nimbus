using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private ISubscriptionClient _subscriptionClient;
        private Queue<Message> _messages = new Queue<Message>();
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
        }

        private async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
        {
            
            if (args.Exception is MessagingEntityNotFoundException exc)
            {
                _logger.Error(exc, "The referenced topic subscription {TopicPath}/{SubscriptionName} no longer exists", _topicPath, _subscriptionName);
                await _queueManager.MarkSubscriptionAsNonExistent(_topicPath, _subscriptionName);
                DiscardSubscriptionClient();
                throw args.Exception;
            }

            _logger.Error(args.Exception, "Messaging operation failed. Discarding message receiver.");
            DiscardSubscriptionClient();
            throw args.Exception;
        }

        private Task OnMessageRecieved(Message message, CancellationToken cancellationToken)
        {
            _messages.Enqueue(message);
            _receiveSemaphore.Release();
            return Task.CompletedTask;
        }

        protected override Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
                            {
                                await _receiveSemaphore.WaitAsync(_pollInterval, cancellationToken);

                                if (_messages.Count == 0)
                                    return null;

                                var message = _messages.Dequeue();

                                var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(message);
                                nimbusMessage.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscriptionName;

                                return nimbusMessage;
                            },
                            cancellationToken).ConfigureAwaitFalse();
            // try
            // {
            //     using (var cancellationSemaphore = new SemaphoreSlim(0, int.MaxValue))
            //     {
            //         
            //         
            //
            //         var receiveTask = subscriptionClient.ReceiveAsync(TimeSpan.FromSeconds(300)).ConfigureAwaitFalse();
            //         var cancellationTask = Task.Run(async () => await CancellationTask(cancellationSemaphore, cancellationToken), cancellationToken).ConfigureAwaitFalse();
            //         await Task.WhenAny(receiveTask, cancellationTask);
            //         if (!receiveTask.IsCompleted) return null;
            //
            //         cancellationSemaphore.Release();
            //
            //         var message = await receiveTask;
            //         if (message == null) return null;
            //
            //         var nimbusMessage = await _messageFactory.BuildNimbusMessage(message);
            //         nimbusMessage.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscriptionName;
            //
            //         return nimbusMessage;
            //     }
            // }
            // catch (MessagingEntityNotFoundException exc)
            // {
            //     _logger.Error(exc, "The referenced topic subscription {TopicPath}/{SubscriptionName} no longer exists", _topicPath, _subscriptionName);
            //     await _queueManager.MarkSubscriptionAsNonExistent(_topicPath, _subscriptionName);
            //     DiscardSubscriptionClient();
            //     throw;
            // }
            // catch (Exception exc)
            // {
            //     _logger.Error(exc, "Messaging operation failed. Discarding message receiver.");
            //     DiscardSubscriptionClient();
            //     throw;
            // }
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
                subscriptionClient.CloseAsync();
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