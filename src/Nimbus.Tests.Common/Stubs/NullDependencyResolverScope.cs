using System;
using Nimbus.InfrastructureContracts.DependencyResolution;

namespace Nimbus.Tests.Common.Stubs
{
    internal class NullDependencyResolverScope : IDependencyResolverScope
    {
        public IDependencyResolverScope CreateChildScope()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public TComponent Resolve<TComponent>()
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type componentType)
        {
            throw new NotImplementedException();
        }
    }
}