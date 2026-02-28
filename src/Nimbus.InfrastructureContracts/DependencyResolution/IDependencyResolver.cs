using System;

namespace Nimbus.InfrastructureContracts.DependencyResolution
{
    public interface IDependencyResolver : ICreateChildScopes, IDisposable
    {
    }
}