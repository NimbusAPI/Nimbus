using System;

namespace Nimbus.DependencyResolution
{
    public interface IDependencyResolverScope : ICreateChildScopes, IDisposable
    {
        TComponent Resolve<TComponent>(string componentName);
        object Resolve(Type componentType, string componentName);
    }
}