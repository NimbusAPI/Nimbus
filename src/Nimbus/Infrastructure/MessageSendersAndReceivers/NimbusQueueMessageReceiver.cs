using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageReceiver : INimbusMessageReceiver
    {
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;
        private readonly ConcurrentHandlerLimitSetting _concurrentHandlerLimit;
        private readonly ILogger _logger;
        private bool _running;

        private readonly object _mutex = new object();
        private Task _workerTask;
        private readonly SemaphoreSlim _throttle;

        public NimbusQueueMessageReceiver(IQueueManager queueManager, string queuePath, ConcurrentHandlerLimitSetting concurrentHandlerLimit, ILogger logger)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
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
            var messageReceiver = await _queueManager.CreateMessageReceiver(_queuePath);
            messageReceiver.PrefetchCount = _concurrentHandlerLimit;

            while (_running)
            {
                try
                {
                    var messages = await messageReceiver.ReceiveBatchAsync(_throttle.CurrentCount, TimeSpan.FromSeconds(300));

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
                    _logger.Error(exc, "Worker exception in {0} for {1}", GetType().Name, _queuePath);
                }
            }

            await messageReceiver.CloseAsync();
        }

        public void Stop()
        {
            if (!_running) return;

            _running = false;

            Task.WaitAll(_workerTask);
        }

        public override string ToString()
        {
            return _queuePath;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            Stop();
            _throttle.Dispose();
        }
    }
}