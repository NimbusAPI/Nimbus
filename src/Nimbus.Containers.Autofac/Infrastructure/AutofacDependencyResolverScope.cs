using System;
using Autofac;
using Nimbus.InfrastructureContracts.DependencyResolution;

namespace Nimbus.Containers.Autofac.Infrastructure
{
    public class AutofacDependencyResolverScope : IDependencyResolverScope
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacDependencyResolverScope(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new AutofacDependencyResolverScope(_lifetimeScope.BeginLifetimeScope());
        }

        public TComponent Resolve<TComponent>()
        {
            return _lifetimeScope.Resolve<TComponent>();
        }

        public object Resolve(Type componentType)
        {
            return _lifetimeScope.Resolve(componentType);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _lifetimeScope.Dispose();
        }
    }
}