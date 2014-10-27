using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.DependencyResolution;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorDependencyResolverScope : IDependencyResolverScope
    {
        private readonly IKernel _kernel;
        private readonly IDisposable _scope;
        private bool _disposed;

        public WindsorDependencyResolverScope(IKernel kernel)
        {
            _kernel = kernel;
            _scope = _kernel.BeginScope();
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new WindsorDependencyResolverScope(_kernel);
        }

        public TComponent Resolve<TComponent>()
        {
            return _kernel.Resolve<TComponent>();
        }

        public object Resolve(Type componentType)
        {
            return _kernel.Resolve(componentType);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (!disposing) return;
            _disposed = true;

            _scope.Dispose();
        }
    }
}