using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

            var tasksClone = tasks.ToArray();
            var queue = new BlockingCollection<TResult>();
            var totalTasks = tasksClone.Count();
            var tasksCompleted = 0;

            foreach (var task in tasksClone)
            {
                task.ContinueWith(t =>
                                  {
                                      if (t.IsFaulted || t.IsCanceled)
                                      {
                                          // just pretend that we've already returned this result so that
                                          // we can return immediately after all the non-broken tasks are
                                          // complete.
                                          Interlocked.Increment(ref tasksCompleted);
                                          return;
                                      }

                                      queue.TryAdd(t.Result);
                                  });
            }

            while (true)
            {
                if (tasksCompleted >= totalTasks) break;
                if (sw.Elapsed >= timeout) break;

                TResult result;
                var remainingTime = timeout - sw.Elapsed;
                if (!queue.TryTake(out result, remainingTime)) continue;

                Interlocked.Increment(ref tasksCompleted);
                yield return result;
            }
        }
    }
}