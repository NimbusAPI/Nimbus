using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Nimbus.Extensions
{
    public static class TaskExtensions
    {
        public static void WaitAll(this IEnumerable<Task> tasks)
        {
            Task.WaitAll(tasks.ToArray());
        }

        public static IEnumerable<TResult> ReturnOpportunistically<TResult>(this IEnumerable<Task<TResult>> tasks, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            var remainingTasks = tasks.ToList();

            while (true)
            {
                if (remainingTasks.None()) yield break;
                if (sw.Elapsed >= timeout) yield break;

                var remainingTasksClone = remainingTasks.ToArray();
                Task.WaitAny(remainingTasksClone, timeout);

                foreach (var task in remainingTasksClone)
                {
                    if (task.IsCompleted)
                    {
                        remainingTasks.Remove(task);

                        if (task.IsFaulted) continue;
                        if (task.IsCanceled) continue;

                        yield return task.Result;
                    }
                }
            }
        }
    }
}