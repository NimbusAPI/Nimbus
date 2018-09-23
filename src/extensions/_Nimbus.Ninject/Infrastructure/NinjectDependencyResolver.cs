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
            _kernel = kernel;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new NinjectDependencyResolverScope(_kernel);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}