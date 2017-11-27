using System;

namespace Nimbus.DependencyResolution
{
    public interface IDependencyResolver : ICreateChildScopes, IDisposable
    {
    }
}