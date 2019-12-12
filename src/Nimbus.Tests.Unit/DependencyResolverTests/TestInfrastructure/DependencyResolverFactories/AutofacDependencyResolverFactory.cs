using Autofac;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.DependencyResolution;

namespace Nimbus.Tests.Unit.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
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