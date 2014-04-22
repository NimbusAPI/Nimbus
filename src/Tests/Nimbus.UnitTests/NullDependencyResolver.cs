using Nimbus.DependencyResolution;

namespace Nimbus.UnitTests
{
    internal class NullDependencyResolver : IDependencyResolver
    {
        public IDependencyResolverScope CreateChildScope()
        {
            return new NullDependencyResolverScope();
        }

        public void Dispose()
        {
        }
    }
}