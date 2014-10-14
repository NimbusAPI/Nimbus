using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.TaskScheduling
{
    public class NimbusTaskFactory
    {
        private readonly Dictionary<ThreadPriority, NimbusTaskScheduler> _schedulers = new Dictionary<ThreadPriority, NimbusTaskScheduler>();

        public NimbusTaskFactory(ILogger logger)
        {
            foreach (var priority in Enum.GetValues(typeof (ThreadPriority)).Cast<ThreadPriority>())
            {
                _schedulers[priority] = new NimbusTaskScheduler(priority, logger);
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
    }
}