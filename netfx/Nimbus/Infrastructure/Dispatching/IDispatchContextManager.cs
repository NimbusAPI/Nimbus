using System;

namespace Nimbus.Infrastructure.Dispatching
{
    internal interface IDispatchContextManager
    {
        IDispatchContext GetCurrentDispatchContext();
        IDisposable StartNewDispatchContext(IDispatchContext dispatchContext);
    }
}