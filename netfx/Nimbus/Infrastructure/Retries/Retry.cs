using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.Retries
{
    internal class Retry : IRetry
    {
        private readonly int _numAttempts;
        private Func<RetryFailureEventArgs, Task> _backoff = args => Task.Delay(0).ConfigureAwaitFalse();

        public EventHandler<RetryEventArgs> Started;
        public EventHandler<RetryEventArgs> Success;
        public EventHandler<RetryFailureEventArgs> TransientFailure;
        public EventHandler<RetryFailureEventArgs> PermanentFailure;

        public Retry(int numAttempts)
        {
            _numAttempts = numAttempts;
        }

        internal TRetry WithBackoff<TRetry>(Func<RetryFailureEventArgs, Task> backoff) where TRetry : Retry
        {
            _backoff = backoff;
            return (TRetry) this;
        }

        public void Do(Action action, string actionName = "")
        {
            var sw = Stopwatch.StartNew();
            Started?.Invoke(this, new RetryEventArgs(actionName, TimeSpan.Zero));
            var attempt = 0;

            while (true)
            {
                attempt++;

                try
                {
                    action();
                    Success?.Invoke(this, new RetryEventArgs(actionName, sw.Elapsed));
                    break;
                }
                catch (Exception exc)
                {
                    var eventArgs = new RetryFailureEventArgs(actionName, attempt, sw.Elapsed, exc);
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, eventArgs);
                        _backoff(eventArgs).Wait();
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, eventArgs);
                        throw;
                    }
                }
            }
        }

        public T Do<T>(Func<T> func, string actionName = "")
        {
            var sw = Stopwatch.StartNew();
            Started?.Invoke(this, new RetryEventArgs(actionName, TimeSpan.Zero));
            var attempt = 0;

            while (true)
            {
                attempt++;

                try
                {
                    var result = func();
                    Success?.Invoke(this, new RetryEventArgs(actionName, sw.Elapsed));
                    return result;
                }
                catch (Exception exc)
                {
                    var eventArgs = new RetryFailureEventArgs(actionName, attempt, sw.Elapsed, exc);
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, eventArgs);
                        _backoff(eventArgs).Wait();
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, eventArgs);
                        throw;
                    }
                }
            }
        }

        public async Task DoAsync(Func<Task> action, string actionName = "")
        {
            var sw = Stopwatch.StartNew();
            Started?.Invoke(this, new RetryEventArgs(actionName, TimeSpan.Zero));
            var attempt = 0;

            while (true)
            {
                attempt++;

                try
                {
                    await action();
                    Success?.Invoke(this, new RetryEventArgs(actionName, sw.Elapsed));
                    break;
                }
                catch (Exception exc)
                {
                    var eventArgs = new RetryFailureEventArgs(actionName, attempt, sw.Elapsed, exc);
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, eventArgs);
                        await _backoff(eventArgs);
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, eventArgs);
                        throw;
                    }
                }
            }
        }

        public async Task<T> DoAsync<T>(Func<Task<T>> func, string actionName = "")
        {
            var sw = Stopwatch.StartNew();
            Started?.Invoke(this, new RetryEventArgs(actionName, TimeSpan.Zero));
            var attempt = 0;

            while (true)
            {
                attempt++;

                try
                {
                    var result = await func();
                    Success?.Invoke(this, new RetryEventArgs(actionName, sw.Elapsed));
                    return result;
                }
                catch (Exception exc)
                {
                    var eventArgs = new RetryFailureEventArgs(actionName, attempt, sw.Elapsed, exc);
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, eventArgs);
                        await _backoff(eventArgs);
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, eventArgs);
                        throw;
                    }
                }
            }
        }
    }
}