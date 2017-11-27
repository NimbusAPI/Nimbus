using System.Collections.Concurrent;
using System.Threading;

namespace Nimbus.Infrastructure
{
    /// <summary>
    /// Re-implementation of CallContext for use with .NET Core / .NET Standard
    /// </summary>
    internal static class CallContext
    {
        private static ConcurrentDictionary<string, AsyncLocal<object>> _data = new ConcurrentDictionary<string, AsyncLocal<object>>();

        internal static void LogicalSetData(string key, object obj)
        {
            _data.GetOrAdd(key, new AsyncLocal<object>()).Value = obj;
        }

        internal static object LogicalGetData(string key)
        {
            AsyncLocal<object> val;

            if (_data.TryGetValue(key, out val))
            {
                return val;
            }

            return null;
        }
    }
}