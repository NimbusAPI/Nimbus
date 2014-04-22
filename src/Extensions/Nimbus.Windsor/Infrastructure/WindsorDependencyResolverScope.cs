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

        public WindsorDependencyResolverScope(IKernel kernel)
        {
            _kernel = kernel;
            _scope = _kernel.BeginScope();
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new WindsorDependencyResolverScope(_kernel);
        }

        public TComponent Resolve<TComponent>(string componentName)
        {
            return _kernel.Resolve<TComponent>(componentName);
        }

        public object Resolve(Type componentType, string componentName)
        {
            return _kernel.Resolve(componentName, componentType);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            _scope.Dispose();
        }
    }
}