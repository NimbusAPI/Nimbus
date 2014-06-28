using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal abstract class ThrottlingMessageReceiver : INimbusMessageReceiver
    {
        protected readonly ConcurrentHandlerLimitSetting ConcurrentHandlerLimit;
        private readonly ILogger _logger;
        private bool _running;

        private Task _workerTask;
        private readonly SemaphoreSlim _throttle;
        private readonly SemaphoreSlim _startStopSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected ThrottlingMessageReceiver(ConcurrentHandlerLimitSetting concurrentHandlerLimit, ILogger logger)
        {
            ConcurrentHandlerLimit = concurrentHandlerLimit;
            _logger = logger;
            _throttle = new SemaphoreSlim(concurrentHandlerLimit, concurrentHandlerLimit);
        }

        public async Task Start(Func<BrokeredMessage, Task> callback)
        {
            try
            {
                await _startStopSemaphore.WaitAsync();

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
            try
            {
                await _startStopSemaphore.WaitAsync();

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

        protected abstract Task<BrokeredMessage[]> FetchBatch(int batchSize, Task cancellationTask);

        private async Task Worker(Func<BrokeredMessage, Task> callback, Task cancellationTask)
        {
            while (_running)
            {
                try
                {
                    var messages = await FetchBatch(_throttle.CurrentCount, cancellationTask);
                    if (!_running) return;
                    if (messages.None()) continue;

                    var tasks = messages
                        .Select(m => Task.Run(async () =>
                                                    {
                                                        try
                                                        {
                                                            await _throttle.WaitAsync(_cancellationTokenSource.Token);
                                                            await callback(m);
                                                        }
                                                        finally
                                                        {
                                                            _throttle.Release();
                                                        }
                                                    }))
                        .ToArray();

                    if (_throttle.CurrentCount == 0) await Task.WhenAny(tasks);
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
                Stop();
                // ReSharper restore CSharpWarnings::CS4014
            }
            catch (ObjectDisposedException)
            {
            }

            _throttle.Dispose();
        }
    }
}