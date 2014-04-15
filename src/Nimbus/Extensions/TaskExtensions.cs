﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.Extensions
{
    internal static class TaskExtensions
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

        internal class OpportunisticTaskCompletionReturner<TResult> : IDisposable
        {
            private bool _disposed;
            private readonly List<Task<TResult>> _remainingTasks;
            private readonly BlockingCollection<TResult> _resultsQueue = new BlockingCollection<TResult>();
            private readonly Task[] _continuations;
            private readonly object _mutex = new object();

            public OpportunisticTaskCompletionReturner(IEnumerable<Task<TResult>> tasks, CancellationTokenSource cancellationToken)
            {
                _remainingTasks = tasks.ToList();

                cancellationToken.Token.Register(Cancel);

                _continuations = _remainingTasks
                    .ToArray()
                    .Select(task => task.ContinueWith(OnTaskCompletion))
                    .ToArray();
            }

            internal Task[] Continuations
            {
                get { return _continuations; }
            }

            public IEnumerable<TResult> GetResults()
            {
                return _resultsQueue.GetConsumingEnumerable();
            }

            private void OnTaskCompletion(Task<TResult> task)
            {
                if (_disposed) return; // already given up before a task completes? just bail.

                lock (_mutex)
                {
                    if (_disposed) return; // already given up before a task completes? just bail.
                    if (_resultsQueue.IsAddingCompleted) return;

                    if (!task.IsFaulted && !task.IsCanceled)
                    {
                        _resultsQueue.Add(task.Result);
                    }

                    _remainingTasks.Remove(task);
                    if (_remainingTasks.None())
                    {
                        _resultsQueue.CompleteAdding();
                    }
                }
            }

            private void Cancel()
            {
                if (_resultsQueue.IsAddingCompleted) return;
                lock (_mutex)
                {
                    if (_resultsQueue.IsAddingCompleted) return;

                    _resultsQueue.CompleteAdding();
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposing) return;
                if (_disposed) return;

                _resultsQueue.CompleteAdding();

                _resultsQueue.Dispose();
                _disposed = true;
            }
        }
    }
}