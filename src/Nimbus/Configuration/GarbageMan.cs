using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Nimbus.Extensions;

namespace Nimbus.Configuration
{
    internal class GarbageMan : IDisposable
    {
        private readonly ConcurrentStack<IDisposable> _trackedComponents = new ConcurrentStack<IDisposable>();

        public void Add(object component)
        {
            var disposable = component as IDisposable;
            if (disposable == null) return;

            Trace.WriteLine("Tracking {0} ({1})".FormatWith(component.GetType().FullName, component.ToString()), "Nimbus.GarbageMan");
            _trackedComponents.Push(disposable);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            IDisposable component;
            while (_trackedComponents.TryPop(out component))
            {
                Trace.WriteLine("Disposing {0} ({1})".FormatWith(component.GetType().FullName, component.ToString()), "Nimbus.GarbageMan");

                try
                {
                    try
                    {
                        component.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                catch (Exception exc)
                {
                    Trace.TraceError(exc.ToString());
                }
            }
        }
    }
}