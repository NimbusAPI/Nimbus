using System;
using Autofac;
using Nimbus.InfrastructureContracts.DependencyResolution;

namespace Nimbus.Containers.Autofac.Infrastructure
{
    public class AutofacDependencyResolver : IDependencyResolver
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacDependencyResolver(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new AutofacDependencyResolverScope(_lifetimeScope.BeginLifetimeScope());
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