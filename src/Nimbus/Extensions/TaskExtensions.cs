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

            var completed = false;

            var tasksClone = tasks.ToArray();
            using (var resultsQueue = new BlockingCollection<TResult>())
            {
                var totalTasks = tasksClone.Count();
                var tasksCompleted = 0;

                foreach (var task in tasksClone)
                {
                    task.ContinueWith(t =>
                                      {
                                          if (completed) return; // just bail if our continuation is invoked after we've already given up.

                                          if (t.IsFaulted || t.IsCanceled)
                                          {
                                              Interlocked.Increment(ref tasksCompleted);
                                              return;
                                          }

                                          resultsQueue.TryAdd(t.Result);
                                      });
                }

                while (true)
                {
                    if (tasksCompleted >= totalTasks) break;
                    if (sw.Elapsed >= timeout) break;

                    TResult result;
                    var remainingTime = timeout - sw.Elapsed;
                    var halfRemainingTime = TimeSpan.FromMilliseconds(remainingTime.TotalMilliseconds/2);
                    if (!resultsQueue.TryTake(out result, halfRemainingTime)) continue;

                    Interlocked.Increment(ref tasksCompleted);
                    yield return result;
                }

                completed = true;
            }
        }
    }
}