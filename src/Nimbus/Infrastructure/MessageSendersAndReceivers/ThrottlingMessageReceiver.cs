using System;
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

        private Task _workerTask;
        private readonly SemaphoreSlim _throttle;
        private readonly SemaphoreSlim _startStopSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected ThrottlingMessageReceiver(ConcurrentHandlerLimitSetting concurrentHandlerLimit, ILogger logger, IGlobalHandlerThrottle globalHandlerThrottle)
        {
            ConcurrentHandlerLimit = concurrentHandlerLimit;
            _logger = logger;
            _globalHandlerThrottle = globalHandlerThrottle;
            _throttle = new SemaphoreSlim(concurrentHandlerLimit, concurrentHandlerLimit);
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
                var cancellationTask = Task.Run(() => { _cancellationTokenSource.Token.WaitHandle.WaitOne(); }, _cancellationTokenSource.Token);

                _workerTask = Task.Run(() => Worker(callback, cancellationTask));
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

                var workerTask = _workerTask;
                _workerTask = null;
                if (workerTask != null) await workerTask;

                // wait for all our existing tasks to complete
                //FIXME a bit ick..
                while (_throttle.CurrentCount < ConcurrentHandlerLimit)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }
            finally
            {
                _startStopSemaphore.Release();
            }
        }

        protected abstract Task WarmUp();

        protected abstract Task<NimbusMessage> Fetch(Task cancellationTask);

        private async Task Worker(Func<NimbusMessage, Task> callback, Task cancellationTask)
        {
            while (_running)
            {
                try
                {
                    await _globalHandlerThrottle.Wait(_cancellationTokenSource.Token);
                    try
                    {
                        await _throttle.WaitAsync(_cancellationTokenSource.Token);
                        try
                        {
                            if (!_running) break;

                            var message = await Fetch(cancellationTask);
                            if (message == null) continue;

                            GlobalMessageCounters.IncrementReceivedMessageCount(1);
                            await callback(message);
                        }
                        finally
                        {
                            _throttle.Release();
                        }
                    }
                    finally
                    {
                        _globalHandlerThrottle.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                    // will be thrown when someone calls .Stop() on us
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

            _throttle.Dispose();
        }
    }
}