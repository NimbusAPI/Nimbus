using System;
using Nimbus.DependencyResolution;
using Ninject;
using Ninject.Activation.Blocks;

namespace Nimbus.Ninject.Infrastructure
{
    public class NinjectDependencyResolverScope : IDependencyResolverScope
    {
        private readonly IKernel _kernel;
        private readonly IActivationBlock _activationBlock;
        private bool _disposed;

        public NinjectDependencyResolverScope(IKernel kernel)
        {
            _kernel = kernel;
            _activationBlock = kernel.BeginBlock();
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new NinjectDependencyResolverScope(_kernel);
        }

        public TComponent Resolve<TComponent>()
        {
            return _activationBlock.Get<TComponent>();
        }

        public object Resolve(Type componentType)
        {
            return _activationBlock.Get(componentType);
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

            _activationBlock.Dispose();
        }
    }
}