using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.DependencyResolution;

namespace Nimbus.Tests.Unit.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public interface IDependencyResolverFactory
    {
        IDependencyResolver Create(ITypeProvider typeProvider);
    }
}