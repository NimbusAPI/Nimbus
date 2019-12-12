using System;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.Dispatching
{
    internal interface IDispatchContextManager
    {
        IDispatchContext GetCurrentDispatchContext();
        IDisposable StartNewDispatchContext(IDispatchContext dispatchContext);
    }
}