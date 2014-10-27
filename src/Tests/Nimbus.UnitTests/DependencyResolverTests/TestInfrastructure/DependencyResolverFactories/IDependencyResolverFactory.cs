using Nimbus.DependencyResolution;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public interface IDependencyResolverFactory
    {
        IDependencyResolver Create(ITypeProvider typeProvider);
    }
}