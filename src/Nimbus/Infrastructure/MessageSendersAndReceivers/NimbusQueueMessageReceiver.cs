using System;
using System.Collections.Generic;
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
        private readonly List<Task> _workerTasks = new List<Task>();

        public NimbusQueueMessageReceiver(IQueueManager queueManager, string queuePath, ConcurrentHandlerLimitSetting concurrentHandlerLimit, ILogger logger)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
            _concurrentHandlerLimit = concurrentHandlerLimit;
            _logger = logger;
        }

        public void Start(Func<BrokeredMessage, Task> callback)
        {
            lock (_mutex)
            {
                if (_running) throw new InvalidOperationException("Already started!");
                _running = true;

                Task.Run(() => Worker(callback));
            }
        }

        private async Task Worker(Func<BrokeredMessage, Task> callback)
        {
            for (var i = 0; i < _concurrentHandlerLimit; i++)
            {
                var workerTask = Task.Run(async () =>
                                                {
                                                    var messageReceiver = _queueManager.CreateMessageReceiver(_queuePath);
                                                    messageReceiver.PrefetchCount = _concurrentHandlerLimit;

                                                    while (_running)
                                                    {
                                                        try
                                                        {
                                                            var message = messageReceiver.Receive(TimeSpan.FromSeconds(5));

                                                            if (message == null) continue;

                                                            await callback(message);
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
                                                });
                _workerTasks.Add(workerTask);
            }
        }

        public void Stop()
        {
            if (!_running) return;

            _running = false;

            Task.WaitAll(_workerTasks.ToArray());
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
        }
    }
}