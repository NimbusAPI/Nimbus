using System;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;

namespace Nimbus.UnitTests.DependencyResolverTests.TestInfrastructure.DependencyResolverFactories
{
    public interface IDependencyResolverFactory : IDisposable
    {
        Task<IDependencyResolver> Create(ITypeProvider typeProvider);
    }
}