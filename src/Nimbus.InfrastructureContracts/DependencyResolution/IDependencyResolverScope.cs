using System;

namespace Nimbus.DependencyResolution
{
    public interface IDependencyResolverScope : ICreateChildScopes, IDisposable
    {
        TComponent Resolve<TComponent>();
        object Resolve(Type componentType);
    }
}