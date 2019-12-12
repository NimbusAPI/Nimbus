using System;

namespace Nimbus.InfrastructureContracts.DependencyResolution
{
    public interface IDependencyResolverScope : ICreateChildScopes, IDisposable
    {
        TComponent Resolve<TComponent>();
        object Resolve(Type componentType);
    }
}