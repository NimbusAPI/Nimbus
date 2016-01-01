using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    internal class RetryEventArgs : EventArgs
    {
        public string ActionName { get; }
        public TimeSpan ElapsedTime { get; }

        public RetryEventArgs(string actionName, TimeSpan elapsedTime)
        {
            ActionName = actionName;
            ElapsedTime = elapsedTime;
        }
    }

    internal class RetryFailureEventArgs : RetryEventArgs
    {
        public Exception Exception { get; }

        public RetryFailureEventArgs(string actionName, TimeSpan elapsedTime, Exception exception) : base(actionName, elapsedTime)
        {
            Exception = exception;
        }
    }

    internal class Retry
    {
        private readonly int _numAttempts;

        public EventHandler<RetryEventArgs> Started;
        public EventHandler<RetryEventArgs> Success;
        public EventHandler<RetryFailureEventArgs> TransientFailure;
        public EventHandler<RetryFailureEventArgs> PermanentFailure;

        public Retry(int numAttempts)
        {
            _numAttempts = numAttempts;
        }

        internal void Do(Action action, string actionName = "")
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
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, new RetryFailureEventArgs(actionName, sw.Elapsed, exc));
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, new RetryFailureEventArgs(actionName, sw.Elapsed, exc));
                        throw;
                    }
                }
            }
        }

        internal T Do<T>(Func<T> func, string actionName = "")
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
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, new RetryFailureEventArgs(actionName, sw.Elapsed, exc));
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, new RetryFailureEventArgs(actionName, sw.Elapsed, exc));
                        throw;
                    }
                }
            }
        }

        internal async Task DoAsync(Func<Task> action, string actionName = "")
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
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, new RetryFailureEventArgs(actionName, sw.Elapsed, exc));
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, new RetryFailureEventArgs(actionName, sw.Elapsed, exc));
                        throw;
                    }
                }
            }
        }

        internal async Task<T> DoAsync<T>(Func<Task<T>> func, string actionName = "")
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
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, new RetryFailureEventArgs(actionName, sw.Elapsed, exc));
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, new RetryFailureEventArgs(actionName, sw.Elapsed, exc));
                        throw;
                    }
                }
            }
        }
    }
}