using System;
using Nimbus.DependencyResolution;

namespace Nimbus.Tests.Common
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

        public TComponent Resolve<TComponent>(string componentName)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type componentType)
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type componentType, string componentName)
        {
            throw new NotImplementedException();
        }
    }
}