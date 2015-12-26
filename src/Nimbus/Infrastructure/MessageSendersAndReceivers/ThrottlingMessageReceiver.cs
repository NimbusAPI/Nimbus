using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal abstract class ThrottlingMessageReceiver : INimbusMessageReceiver
    {
        protected readonly ConcurrentHandlerLimitSetting ConcurrentHandlerLimit;
        private readonly ILogger _logger;
        private readonly IGlobalHandlerThrottle _globalHandlerThrottle;
        private bool _running;

        private Task[] _workerTasks;
        private readonly SemaphoreSlim _startStopSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected ThrottlingMessageReceiver(ConcurrentHandlerLimitSetting concurrentHandlerLimit, IGlobalHandlerThrottle globalHandlerThrottle, ILogger logger)
        {
            ConcurrentHandlerLimit = concurrentHandlerLimit;
            _logger = logger;
            _globalHandlerThrottle = globalHandlerThrottle;
        }

        public async Task Start(Func<NimbusMessage, Task> callback)
        {
            await _startStopSemaphore.WaitAsync();

            try
            {
                if (_running) return;
                _running = true;

                await WarmUp();

                _cancellationTokenSource = new CancellationTokenSource();

                _workerTasks = Enumerable.Range(0, ConcurrentHandlerLimit)
                                         .Select(i => Task.Run(() => Worker(callback), _cancellationTokenSource.Token))
                                         .ToArray();
            }
            finally
            {
                _startStopSemaphore.Release();
            }
        }

        public async Task Stop()
        {
            await _startStopSemaphore.WaitAsync();

            try
            {
                if (!_running) return;
                _running = false;

                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

                var workerTasks = _workerTasks.ToArray();
                _workerTasks = null;
                await Task.WhenAll(workerTasks);
            }
            finally
            {
                _startStopSemaphore.Release();
            }
        }

        protected abstract Task WarmUp();

        protected abstract Task<NimbusMessage> Fetch(CancellationToken cancellationToken);

        private async Task Worker(Func<NimbusMessage, Task> callback)
        {
            while (true)
            {
                if (!_running) break;
                if (_cancellationTokenSource.IsCancellationRequested) break;

                try
                {
                    var message = await Fetch(_cancellationTokenSource.Token);
                    if (message == null) continue;

                    await _globalHandlerThrottle.Wait(_cancellationTokenSource.Token);
                    try
                    {
                        GlobalMessageCounters.IncrementReceivedMessageCount(1);
                        await callback(message);
                    }
                    finally
                    {
                        _globalHandlerThrottle.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                    // will be thrown when someone calls .Stop() on us
                    break;
                }
                catch (Exception exc)
                {
                    _logger.Error(exc, "Worker exception in {0} for {1}", GetType().Name, this);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            try
            {
                // ReSharper disable CSharpWarnings::CS4014
#pragma warning disable 4014
                Stop();
#pragma warning restore 4014
                // ReSharper restore CSharpWarnings::CS4014
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}