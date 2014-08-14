using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.TaskScheduling
{
    public class PriorityScheduler : TaskScheduler
    {
        public static PriorityScheduler Highest = new PriorityScheduler(ThreadPriority.Highest, Environment.ProcessorCount);
        public static PriorityScheduler BelowNormal = new PriorityScheduler(ThreadPriority.BelowNormal, Math.Max(1, Environment.ProcessorCount/2));
        public static PriorityScheduler Lowest = new PriorityScheduler(ThreadPriority.Lowest, Math.Max(1, Environment.ProcessorCount/2));

        private readonly BlockingCollection<Task> _tasks = new BlockingCollection<Task>();
        private readonly int _maximumConcurrencyLevel;

        public PriorityScheduler(ThreadPriority priority, int maximumConcurrencyLevel)
        {
            _maximumConcurrencyLevel = maximumConcurrencyLevel;

            for (var i = 0; i < _maximumConcurrencyLevel; i++)
            {
                new Thread(() =>
                           {
                               foreach (var t in _tasks.GetConsumingEnumerable())
                               {
                                   TryExecuteTask(t);
                               }
                           })
                {
                    Name = string.Format("PriorityScheduler: {0}/{1}", priority, i),
                    Priority = priority,
                    IsBackground = false,
                }.Start();
            }
        }

        public override int MaximumConcurrencyLevel
        {
            get { return _maximumConcurrencyLevel; }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks;
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false; // we might not want to execute task that should schedule as high or low priority inline
        }
    }
}