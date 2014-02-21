using System;
using System.Linq;

namespace Nimbus.Infrastructure
{
    /// <summary>
    /// Used to encapsulate a bunch of IDisposable objects inside a single wrapper so that we can dispose of them cleanly
    /// at the end of a message-handling unit of work.
    /// </summary>
    internal class DisposableWrapper : IDisposable
    {
        private readonly object[] _components;

        public DisposableWrapper(params object[] components)
        {
            _components = components;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            foreach (var component in _components.OfType<IDisposable>())
            {
                try
                {
                    component.Dispose();
                }
                catch { }
            }
        }
    }
}