using System;

namespace Nimbus.InfrastructureContracts
{
    public class OwnedComponent<T> : IDisposable
    {
        private readonly T _component;
        private readonly IDisposable _componentScope;

        public OwnedComponent(T component, IDisposable componentScope = null)
        {
            _component = component;
            _componentScope = componentScope;
        }

        public T Component
        {
            get { return _component; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_componentScope == null) return;

            _componentScope.Dispose();
        }
    }
}