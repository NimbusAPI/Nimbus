using System;

namespace Nimbus.Infrastructure.Retries
{
    internal class RetryFailureEventArgs : RetryEventArgs
    {
        public int FailedAttempts { get; }
        public Exception Exception { get; }

        public RetryFailureEventArgs(string actionName, int failedAttempts, TimeSpan elapsedTime, Exception exception) : base(actionName, elapsedTime)
        {
            FailedAttempts = failedAttempts;
            Exception = exception;
        }
    }
}