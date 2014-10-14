using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.TaskScheduling
{
    public class NimbusTaskScheduler : TaskScheduler
    {
        private readonly ThreadPriority _priority;
        private readonly ILogger _logger;
        private readonly List<Task> _tasks = new List<Task>();
        private readonly List<Thread> _threads = new List<Thread>();
        private readonly SemaphoreSlim _tasksSemaphore = new SemaphoreSlim(0, int.MaxValue);
        private readonly int _minThreadCount;
        private readonly int _maxThreadCount;
        private int _currentThreadCount;
        private int _currentExecutingTasks;

        public NimbusTaskScheduler(ThreadPriority priority, ILogger logger)
        {
            _priority = priority;
            _logger = logger;

            //var scale = Convert.ToInt32(priority) + 1;
            var scale = 1;
            _minThreadCount = Environment.ProcessorCount*scale;
            _maxThreadCount = Environment.ProcessorCount*scale*5;

            Enumerable.Range(0, _maxThreadCount)
                      .AsParallel()
                      .Do(i => AddThread())
                      .Done();
        }

        private void AddThread()
        {
            var thread = new Thread(Worker)
                         {
                             Name = ToString(),
                             IsBackground = true,
                             Priority = _priority,
                         };

            lock (_threads)
            {
                _threads.Add(thread);
                _currentThreadCount++;
                thread.Start();

                Log("Added", thread);
            }
        }

        private void RemoveThread(Thread thread)
        {
            lock (_threads)
            {
                _threads.Remove(Thread.CurrentThread);
                _currentThreadCount--;
                Log("Removed", thread);
            }
        }

        private void Worker()
        {
            while (true)
            {
                try
                {
                    Task task = null;

                    _tasksSemaphore.Wait();

                    lock (_tasks)
                    {
                        task = _tasks[0];
                        _tasks.RemoveAt(0);
                    }

                    var executingTaskCount = Interlocked.Increment(ref _currentExecutingTasks);
                    if ((executingTaskCount == _currentThreadCount) && (_currentThreadCount < _maxThreadCount)) AddThread();
                    TryExecuteTask(task);
                    Interlocked.Decrement(ref _currentExecutingTasks);

                    //if ((executingTaskCount < _minThreadCount) && (_currentThreadCount > _minThreadCount)) break;
                }
                catch (Exception exc)
                {
                    _logger.Error(exc, exc.Message);
                    break;
                }
            }

            RemoveThread(Thread.CurrentThread);
        }

        protected override void QueueTask(Task task)
        {
            lock (_tasks)
            {
                _tasks.Insert(0, task);
            }

            _tasksSemaphore.Release();
        }

        private void Log(string action, Thread thread)
        {
            _logger.Info("{ThreadPoolAction} thread ID {ThreadId} to/from pool for scheduler {SchedulerName} ({TotalThreads} total threads)", action, thread.ManagedThreadId, ToString(), _currentThreadCount);
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
    }
}