using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusSubscriptionMessageReceiver : INimbusMessageReceiver
    {
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private readonly ConcurrentHandlerLimitSetting _concurrentHandlerLimit;
        private readonly ILogger _logger;

        private SubscriptionClient _subscriptionClient;
        private readonly object _mutex = new object();
        private bool _running;
        private readonly SemaphoreSlim _throttle;
        private Task _workerTask;

        public NimbusSubscriptionMessageReceiver(IQueueManager queueManager,
                                                 string topicPath,
                                                 string subscriptionName,
                                                 ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                                 ILogger logger)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _subscriptionName = subscriptionName;
            _concurrentHandlerLimit = concurrentHandlerLimit;
            _logger = logger;
            _throttle = new SemaphoreSlim(concurrentHandlerLimit, concurrentHandlerLimit);
        }

        public void Start(Func<BrokeredMessage, Task> callback)
        {
            lock (_mutex)
            {
                if (_running) throw new InvalidOperationException("Already started!");
                _running = true;

                _workerTask = Task.Run(() => Worker(callback));
            }
        }

        private async Task Worker(Func<BrokeredMessage, Task> callback)
        {
            var subscriptionClient = await _queueManager.CreateSubscriptionReceiver(_topicPath, _subscriptionName);
            subscriptionClient.PrefetchCount = _concurrentHandlerLimit;

            while (_running)
            {
                try
                {
                    var messages = await subscriptionClient.ReceiveBatchAsync(_throttle.CurrentCount, TimeSpan.FromSeconds(300));

                    var tasks = messages
                        .Select(async m =>
                                      {
                                          try
                                          {
                                              await _throttle.WaitAsync();
                                              await callback(m);
                                          }
                                          finally
                                          {
                                              _throttle.Release();
                                          }
                                      })
                        .ToArray();

                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException)
                {
                    // will be thrown when someone calls .Stop() on us
                }
                catch (Exception exc)
                {
                    _logger.Error(exc, "Worker exception in {0} for {1}/{2}", GetType().Name, _topicPath, _subscriptionName);
                }
            }

            await subscriptionClient.CloseAsync();
        }

        public void Stop()
        {
            if (!_running) return;

            _running = false;

            Task.WaitAll(_workerTask);
        }

        public override string ToString()
        {
            return "{0}/{1}".FormatWith(_topicPath, _subscriptionName);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}