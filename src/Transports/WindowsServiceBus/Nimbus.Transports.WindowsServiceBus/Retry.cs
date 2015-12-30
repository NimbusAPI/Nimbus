using System;
using System.Threading.Tasks;

namespace Nimbus.Transports.WindowsServiceBus
{
    internal class Retry
    {
        private readonly int _numAttempts;

        public EventHandler<Exception> TransientFailure;
        public EventHandler<Exception> PermanentFailure;

        public Retry(int numAttempts)
        {
            _numAttempts = numAttempts;
        }

        internal void Do(Action action)
        {
            var attempt = 0;
            while (true)
            {
                attempt++;

                try
                {
                    action();
                    break;
                }
                catch (Exception exc)
                {
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, exc);
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, exc);
                        throw;
                    }
                }
            }
        }

        internal T Do<T>(Func<T> func)
        {
            var attempt = 0;
            while (true)
            {
                attempt++;

                try
                {
                    return func();
                }
                catch (Exception exc)
                {
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, exc);
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, exc);
                        throw;
                    }
                }
            }
        }

        internal async Task DoAsync(Func<Task> action)
        {
            var attempt = 0;
            while (true)
            {
                attempt++;

                try
                {
                    await action();
                    break;
                }
                catch (Exception exc)
                {
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, exc);
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, exc);
                        throw;
                    }
                }
            }
        }

        internal async Task<T> DoAsync<T>(Func<Task<T>> func)
        {
            var attempt = 0;
            while (true)
            {
                attempt++;

                try
                {
                    return await func();
                }
                catch (Exception exc)
                {
                    if (attempt < _numAttempts)
                    {
                        TransientFailure?.Invoke(this, exc);
                    }
                    else
                    {
                        PermanentFailure?.Invoke(this, exc);
                        throw;
                    }
                }
            }
        }
    }
}