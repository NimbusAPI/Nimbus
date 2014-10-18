using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.TaskScheduling
{
    internal class NimbusTaskFactory : INimbusTaskFactory, IDisposable
    {
        private readonly Dictionary<ThreadPriority, NimbusTaskScheduler> _schedulers = new Dictionary<ThreadPriority, NimbusTaskScheduler>();
        private bool _disposed;

        public NimbusTaskFactory(ILogger logger, MinimumThreadPoolThreadsSetting minimumThreadPoolThreads, MaximumThreadPoolThreadsSetting maximumThreadPoolThreads)
        {
            foreach (var priority in Enum.GetValues(typeof (ThreadPriority)).Cast<ThreadPriority>())
            {
                _schedulers[priority] = new NimbusTaskScheduler(priority, logger, minimumThreadPoolThreads, maximumThreadPoolThreads);
            }
        }

        public Task StartNew(Action taskAction, ThreadPriority priority)
        {
            var scheduler = _schedulers[priority];
            var task = Task.Factory.StartNew(taskAction, default(CancellationToken), TaskCreationOptions.None, scheduler);
            return task;
        }

        public Task<TResult> StartNew<TResult>(Func<TResult> taskFunc, ThreadPriority priority)
        {
            var scheduler = _schedulers[priority];
            var task = Task.Factory.StartNew(taskFunc, default(CancellationToken), TaskCreationOptions.None, scheduler);
            return task;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            _schedulers.Values
                       .Do(s => s.Dispose())
                       .Done();

            _disposed = true;
        }
    }
}