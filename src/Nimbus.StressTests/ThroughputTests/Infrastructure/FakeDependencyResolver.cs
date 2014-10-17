using Nimbus.DependencyResolution;
using Nimbus.StressTests.ThroughputTests.EventHandlers;

namespace Nimbus.StressTests.ThroughputTests.Infrastructure
{
    public class FakeDependencyResolver : IDependencyResolver
    {
        private readonly FakeHandler _fakeHandler;

        public FakeDependencyResolver(FakeHandler fakeHandler)
        {
            _fakeHandler = fakeHandler;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new FakeChildScope(_fakeHandler);
        }

        public void Dispose()
        {
        }
    }
}