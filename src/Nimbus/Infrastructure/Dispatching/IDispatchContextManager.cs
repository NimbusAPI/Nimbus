using System;

namespace Nimbus.Infrastructure.Dispatching
{
    public interface IDispatchContextManager
    {
        IDispatchContext GetCurrentDispatchContext();
        IDisposable StartNewDispatchContext(IDispatchContext dispatchContext);
    }
}