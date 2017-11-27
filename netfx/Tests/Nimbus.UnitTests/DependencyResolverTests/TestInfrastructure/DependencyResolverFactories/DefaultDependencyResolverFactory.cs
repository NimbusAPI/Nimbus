using Nimbus.DependencyResolution;
using Nimbus.Infrastructure.DependencyResolution;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
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