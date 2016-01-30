using System;
using Nimbus.Configuration.PoorMansIocContainer;

namespace Nimbus.Infrastructure
{
    internal abstract class MessagePumpFactory : IDisposable
    {
        protected readonly GarbageMan GarbageMan = new GarbageMan();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            GarbageMan.Dispose();
        }
    }
}