using System;
using Nimbus.DependencyResolution;
using Ninject;
using Ninject.Activation.Blocks;

namespace Nimbus.Ninject.Infrastructure
{
    public class NinjectDependencyResolverScope : IDependencyResolverScope
    {
        private readonly IKernel _kernel;

        private IActivationBlock _activationBlock;

        public NinjectDependencyResolverScope(IKernel kernel)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException("kernel");
            }

            _kernel = kernel;

            _activationBlock = _kernel.BeginBlock();
        }

        ~NinjectDependencyResolverScope()
        {
            Dispose(false);
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new NinjectDependencyResolverScope(_kernel);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public TComponent Resolve<TComponent>(string componentName)
        {
            return _kernel.Get<TComponent>(componentName);
        }

        public object Resolve(Type componentType)
        {
            return _kernel.Get(componentType);
        }

        public object Resolve(Type componentType, string componentName)
        {
            return _kernel.Get(componentType, componentName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_activationBlock != null)
                {
                    _activationBlock.Dispose();
                    _activationBlock = null;
                }
            }
        }
    }
}