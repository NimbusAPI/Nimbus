using System;
using Nimbus.DependencyResolution;
using Nimbus.IntegrationTests.Tests.ThroughputTests.EventHandlers;

namespace Nimbus.IntegrationTests.Tests.ThroughputTests.Infrastructure
{
    public class FakeChildScope : IDependencyResolverScope
    {
        private readonly FakeHandler _fakeHandler;

        public FakeChildScope(FakeHandler fakeHandler)
        {
            _fakeHandler = fakeHandler;
        }

        public IDependencyResolverScope CreateChildScope()
        {
            return new FakeChildScope(_fakeHandler);
        }

        public TComponent Resolve<TComponent>(string componentName)
        {
            return (TComponent) (object) _fakeHandler;
        }

        public object Resolve(Type componentType)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type componentType, string componentName)
        {
            throw new NotImplementedException();
        }

        public TComponent[] ResolveAll<TComponent>()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}