using Autofac;
using Nimbus.DependencyResolution;

namespace Nimbus.Autofac.Infrastructure
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

        public TComponent Resolve<TComponent>(string componentName)
        {
            return _lifetimeScope.ResolveNamed<TComponent>(componentName);
        }

        public TComponent[] ResolveAll<TComponent>()
        {
            return _lifetimeScope.Resolve<TComponent[]>();
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