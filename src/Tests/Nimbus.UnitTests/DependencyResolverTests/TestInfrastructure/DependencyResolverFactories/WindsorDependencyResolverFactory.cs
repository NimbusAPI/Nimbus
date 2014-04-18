using System;
using System.Threading.Tasks;
using Castle.Windsor;
using Nimbus.DependencyResolution;
using Nimbus.Windsor.Configuration;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public class WindsorDependencyResolverFactory : IDependencyResolverFactory
    {
        private IWindsorContainer _container;

        public async Task<IDependencyResolver> Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);
            return _container.Resolve<IDependencyResolver>();
        }

        private void BuildContainer(ITypeProvider typeProvider)
        {
            if (_container != null) throw new InvalidOperationException("This factory is only supposed to be used to construct one test subject.");

            _container = new WindsorContainer();
            _container.RegisterNimbus(typeProvider);
        }

        public void Dispose()
        {
            var container = _container;
            if (container == null) return;
            container.Dispose();
        }
    }
}