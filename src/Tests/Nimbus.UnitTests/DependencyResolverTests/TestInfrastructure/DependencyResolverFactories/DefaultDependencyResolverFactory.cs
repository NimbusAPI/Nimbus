using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Infrastructure.DependencyResolution;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public class DefaultDependencyResolverFactory : IDependencyResolverFactory
    {
        public void Dispose()
        {
        }

        public async Task<IDependencyResolver> Create(ITypeProvider typeProvider)
        {
            return new DependencyResolver(typeProvider);
        }
    }
}