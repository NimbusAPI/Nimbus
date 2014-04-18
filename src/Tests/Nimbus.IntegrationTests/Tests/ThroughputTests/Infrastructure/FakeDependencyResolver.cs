using Nimbus.DependencyResolution;
using Nimbus.IntegrationTests.Tests.ThroughputTests.EventHandlers;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests.Infrastructure
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