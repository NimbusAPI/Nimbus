using Autofac;
using Nimbus.Configuration;
using Nimbus.DependencyResolution;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public class AutofacDependencyResolverFactory : IDependencyResolverFactory
    {
        public IDependencyResolver Create(ITypeProvider typeProvider)
        {
            var builder = new ContainerBuilder();
            builder.RegisterNimbus(typeProvider);
            return builder.Build().Resolve<IDependencyResolver>();
        }
    }
}