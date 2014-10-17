using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.TaskScheduling
{
    internal class NimbusTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly ThreadPriority _priority;
        private readonly ILogger _logger;
        private readonly List<Task> _tasks = new List<Task>();
        private readonly SemaphoreSlim _tasksSemaphore = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _watcherSemaphore = new SemaphoreSlim(0);
        private readonly int _minThreadCount;
        private readonly int _maxThreadCount;
        private int _currentThreadCount;
        private int _shouldKillOneThread;
        private bool _disposed;
        private readonly CancellationTokenSource _disposedCancellationTokenSource = new CancellationTokenSource();

        private int _currentExecutingTaskCount;
        private int _movingAverageExecutingTaskCount;

        private const int _scaleUpWhenWeHitPercentage = 60;
        private const int _scaleDownWhenWeHitPercentage = 20;
        public const string LogTaskSchedulerCompilerDirective = "LogTaskScheduler";

        public NimbusTaskScheduler(ThreadPriority priority, ILogger logger)
        {
            _priority = priority;
            _logger = logger;

            var absoluteMinimumThreadCount = Math.Max(Environment.ProcessorCount, 4);
            _minThreadCount = absoluteMinimumThreadCount*2;
            _maxThreadCount = absoluteMinimumThreadCount*absoluteMinimumThreadCount;

            var watcher = new Thread(Watcher)
                          {
                              Name = ToString() + "Watcher",
                              IsBackground = true,
                              Priority = ThreadPriority.Highest,
                          };
            watcher.Start();
        }

        private void Watcher()
        {
            while (!_disposed)
            {
                var percentageOfThreadPoolInUse = CalculatePercentageOfThreadPoolInUse();

                if (percentageOfThreadPoolInUse > _scaleUpWhenWeHitPercentage)
                {
                    // If we need more threads, we probably need them in a hurry :p
                    var deficit = Environment.ProcessorCount;
                    if (_currentThreadCount + deficit > _maxThreadCount) deficit = _maxThreadCount - _currentThreadCount;

                    for (var i = 0; i < deficit; i++) AddOneThread();
                }
                else if (percentageOfThreadPoolInUse < _scaleDownWhenWeHitPercentage)
                {
                    // If we have too many threads, let's scale them back down gently so that we don't fluctuate like a crazy thing.
                    if (_currentThreadCount > _minThreadCount)
                    {
                        RemoveOneThread();
                    }
                }

                _watcherSemaphore.Wait();
            }
        }

        private void Worker()
        {
            Interlocked.Increment(ref _currentThreadCount);
            Log("Added", Thread.CurrentThread);

            try
            {
                while (true)
                {
                    try
                    {
                        _tasksSemaphore.Wait(_disposedCancellationTokenSource.Token);
                        if (_disposed) break;

                        Task task;
                        lock (_tasks)
                        {
                            task = _tasks[0];
                            _tasks.RemoveAt(0);
                        }

                        Interlocked.Increment(ref _currentExecutingTaskCount);
                        _movingAverageExecutingTaskCount = (_currentExecutingTaskCount + _movingAverageExecutingTaskCount + _tasks.Count)/2;
                        _watcherSemaphore.Release();
                        try
                        {
                            TryExecuteTask(task);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _currentExecutingTaskCount);
                        }

                        var shouldKillThisThread = Interlocked.Exchange(ref _shouldKillOneThread, 0);
                        if (shouldKillThisThread != 0) break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception exc)
                    {
                        _logger.Error(exc, exc.Message);
                        break;
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref _currentThreadCount);
                Log("Removed", Thread.CurrentThread);
            }
        }

        private int CalculatePercentageOfThreadPoolInUse()
        {
            var currentThreadCount = _currentThreadCount;
            if (currentThreadCount == 0) return 100;

            var movingAverageExecutingTaskCount = _movingAverageExecutingTaskCount;
            var percentageOfThreadPoolInUse = movingAverageExecutingTaskCount*100/currentThreadCount;

            LogThreadPoolUtilization(percentageOfThreadPoolInUse, movingAverageExecutingTaskCount, currentThreadCount);

            return percentageOfThreadPoolInUse;
        }

        private void AddOneThread()
        {
            var thread = new Thread(Worker)
                         {
                             Name = ToString(),
                             IsBackground = true,
                             Priority = _priority,
                         };
            thread.Start();
        }

        private void RemoveOneThread()
        {
            _shouldKillOneThread = 1;
        }

        protected override void QueueTask(Task task)
        {
            lock (_tasks)
            {
                _tasks.Insert(0, task);
            }

            _tasksSemaphore.Release();
        }

        [Conditional(LogTaskSchedulerCompilerDirective)]
        private void LogThreadPoolUtilization(int percentageOfThreadPoolInUse, int movingAverageExecutingTaskCount, int currentThreadCount)
        {
            _logger.Debug("{ThreadPoolPercentageInUse} percent of {ThreadPriority} thread pool in use (average of {ExecutingTaskCount} tasks executing on {ThreadCount} threads)",
                          percentageOfThreadPoolInUse,
                          _priority,
                          movingAverageExecutingTaskCount,
                          currentThreadCount);
        }

        [Conditional(LogTaskSchedulerCompilerDirective)]
        private void Log(string action, Thread thread)
        {
            _logger.Info("{ThreadPoolAction} thread ID {ThreadId} to/from pool for scheduler {SchedulerName} ({TotalThreads} total threads)",
                         action,
                         thread.ManagedThreadId,
                         ToString(),
                         _currentThreadCount);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            yield break;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", GetType().Name, _priority);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            _disposed = true;
            _disposedCancellationTokenSource.Cancel();
        }
    }
}