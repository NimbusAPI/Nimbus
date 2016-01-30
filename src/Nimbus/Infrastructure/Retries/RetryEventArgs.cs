using System;

namespace Nimbus.Infrastructure.Retries
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
}