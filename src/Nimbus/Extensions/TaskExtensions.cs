using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            using (var cancellationTokenSource = new CancellationTokenSource(timeout))
            {
                using (var returner = new OpportunisticTaskCompletionReturner<TResult>(tasks, cancellationTokenSource))
                {
                    foreach (var result in returner.GetResults())
                    {
                        yield return result;
                    }
                }
            }
        }
    }

    internal class OpportunisticTaskCompletionReturner<TResult> : IDisposable
    {
        private bool _disposed;
        private readonly List<Task<TResult>> _remainingTasks;
        private readonly BlockingCollection<TResult> _resultsQueue = new BlockingCollection<TResult>();

        public OpportunisticTaskCompletionReturner(IEnumerable<Task<TResult>> tasks, CancellationTokenSource cancellationToken)
        {
            _remainingTasks = tasks.ToList();

            foreach (var task in _remainingTasks.ToArray()) task.ContinueWith(OnTaskCompletion, TaskContinuationOptions.ExecuteSynchronously);
            cancellationToken.Token.Register(() => _resultsQueue.CompleteAdding());
        }

        public IEnumerable<TResult> GetResults()
        {
            return _resultsQueue.GetConsumingEnumerable();
        }

        private void OnTaskCompletion(Task<TResult> task)
        {
            if (_disposed) return; // already given up before a task completes? just bail.

            lock (_remainingTasks)
            {
                _remainingTasks.Remove(task);
                if (task.IsFaulted) return;
                if (task.IsCanceled) return;
                _resultsQueue.Add(task.Result);

                if (_remainingTasks.None()) _resultsQueue.CompleteAdding();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _disposed = true;

            _resultsQueue.Dispose();
        }
    }
}