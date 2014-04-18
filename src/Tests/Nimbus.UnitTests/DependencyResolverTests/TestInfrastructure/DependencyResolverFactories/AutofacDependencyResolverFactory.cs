using System;
using System.Threading.Tasks;
using Autofac;
using Nimbus.Configuration;
using Nimbus.DependencyResolution;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public class AutofacDependencyResolverFactory : IDependencyResolverFactory
    {
        private IContainer _container;

        public async Task<IDependencyResolver> Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);
            return _container.Resolve<IDependencyResolver>();
        }

        private void BuildContainer(ITypeProvider typeProvider)
        {
            if (_container != null) throw new InvalidOperationException("This factory is only supposed to be used to construct one test subject.");
            var builder = new ContainerBuilder();
            builder.RegisterNimbus(typeProvider);
            _container = builder.Build();
        }

        public void Dispose()
        {
            var container = _container;
            if (container == null) return;
            container.Dispose();
        }
    }
}