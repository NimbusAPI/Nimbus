using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.ConcurrentCollections
{
    public class ThreadSafeDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        public IDictionary<TKey, TValue> ToDictionary()
        {
            return this.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
