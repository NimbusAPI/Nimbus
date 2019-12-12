using System;
using System.Threading;

namespace Nimbus.Tests.Common.TestUtilities
{
    public sealed class SingleThreadedSynchronizationContext : SynchronizationContext, IDisposable
    {
        private readonly SynchronizationContext _previousContext;
        private readonly object _mutex = new object();

        public SingleThreadedSynchronizationContext()
        {
            _previousContext = Current;
            SetSynchronizationContext(this);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            lock (_mutex)
            {
                d(state);
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            lock (_mutex)
            {
                d(state);
            }
        }

        public void Dispose()
        {
            SetSynchronizationContext(_previousContext);
        }
    }
}