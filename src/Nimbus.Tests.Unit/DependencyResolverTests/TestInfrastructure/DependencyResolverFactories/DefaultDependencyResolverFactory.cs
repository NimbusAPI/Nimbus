using Nimbus.Infrastructure.DependencyResolution;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.DependencyResolution;

namespace Nimbus.Tests.Unit.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public class DefaultDependencyResolverFactory : IDependencyResolverFactory
    {
        public IDependencyResolver Create(ITypeProvider typeProvider)
        {
            return new DependencyResolver(typeProvider);
        }

        public void Dispose()
        {
        }
    }
}