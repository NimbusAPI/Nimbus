using System;
using Nimbus.DependencyResolution;
using Ninject;

namespace Nimbus.Ninject.Infrastructure
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectDependencyResolver(IKernel kernel)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException("kernel");
            }

            _kernel = kernel;
        }

        ~NinjectDependencyResolver()
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

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}