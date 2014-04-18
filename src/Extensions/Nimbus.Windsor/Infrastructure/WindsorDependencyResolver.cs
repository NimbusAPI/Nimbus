using Castle.MicroKernel;
using Castle.Windsor;
using Nimbus.DependencyResolution;
using IDependencyResolver = Nimbus.DependencyResolution.IDependencyResolver;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        public WindsorDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new WindsorDependencyResolverScope(_kernel);
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