using System;
using System.Runtime.Remoting.Messaging;
using Nimbus.ConcurrentCollections;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.Dispatching
{
    internal class DispatchContextManager : IDispatchContextManager
    {
        private static readonly string _currentDispatchIdDataSlotName = typeof(SubsequentDispatchContext).FullName;

        private readonly ThreadSafeDictionary<string, SubsequentDispatchContext> _store = new ThreadSafeDictionary<string, SubsequentDispatchContext>();

        public IDispatchContext GetCurrentDispatchContext()
        {
            // Try to get the current DispatchContext if there is one, or happily return a new NullDispatchContext
            var currentDispatchContextId = GetCurrentDispatchContextId();
            if (currentDispatchContextId == null) return new InitialDispatchContext();

            SubsequentDispatchContext dispatchContext;
            return (_store.TryGetValue(currentDispatchContextId, out dispatchContext))
                ? (IDispatchContext) dispatchContext
                : new InitialDispatchContext();
        }

        public IDisposable StartNewDispatchContext(IDispatchContext dispatchContext)
        {
            AssertCanStartDispatch(dispatchContext);
            
            if (!_store.TryAdd(dispatchContext.DispatchId, (SubsequentDispatchContext) dispatchContext))
                throw new InvalidOperationException("Cannot add duplicate {0} {1} to the {0} store."
                                                        .FormatWith(typeof(SubsequentDispatchContext).Name, dispatchContext.DispatchId));

            SetCurrentDispatchId(dispatchContext.DispatchId);

            return new DispatchContextWrapper(this, dispatchContext.DispatchId);
        }

        private void CompleteDispatchContext(string dispatchContextId)
        {
            var currentDispatchId = GetCurrentDispatchContextId();
            if (dispatchContextId != currentDispatchId)
                throw new InvalidOperationException("You are trying to stop the Dispatch {0} when the current Dispatch is {1}"
                                                        .FormatWith(dispatchContextId, currentDispatchId));

            ClearCurrentDispatchId();

            SubsequentDispatchContext removed;
            _store.TryRemove(dispatchContextId, out removed);
        }

// ReSharper disable once UnusedParameter.Local
        private static void AssertCanStartDispatch(IDispatchContext dispatchContext)
        {
            var currentDispatchId = GetCurrentDispatchContextId();
            if (currentDispatchId != null)
                throw new InvalidOperationException("Dispatch {0} is already in progress in this Logical CallContext. Did you forget to Dispose it?"
                                                        .FormatWith(currentDispatchId));
            
            if (dispatchContext is InitialDispatchContext)
                throw new InvalidOperationException("Don't start a Dispatch with a {0}, use a new {1} instead."
                                                        .FormatWith(typeof(InitialDispatchContext).Name, typeof(SubsequentDispatchContext).Name));
        }

        private static void ClearCurrentDispatchId()
        {
            CallContext.LogicalSetData(_currentDispatchIdDataSlotName, null);
        }

        private static void SetCurrentDispatchId(string dispatchId)
        {
            CallContext.LogicalSetData(_currentDispatchIdDataSlotName, dispatchId);
        }

        private static string GetCurrentDispatchContextId()
        {
            return CallContext.LogicalGetData(_currentDispatchIdDataSlotName) as string;
        }

        private sealed class DispatchContextWrapper : IDisposable
        {
            private readonly DispatchContextManager _owner;
            private readonly string _dispatchContextId;

            public DispatchContextWrapper(DispatchContextManager owner, string dispatchContextId)
            {
                _owner = owner;
                _dispatchContextId = dispatchContextId;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            private void Dispose(bool disposing)
            {
                if (!disposing) return;
                _owner.CompleteDispatchContext(_dispatchContextId);
            }
        }
    }
}