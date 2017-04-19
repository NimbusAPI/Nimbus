using System;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.ConcurrentCollections
{
    public class ThreadSafeDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        private readonly IDictionary<TKey, object> _locks = new Dictionary<TKey, object>();

        public TValue this[TKey key]
        {
            get
            {
                lock (_dictionary)
                {
                    return _dictionary[key];
                }
            }
            set
            {
                lock (LockForKey(key))
                {
                    lock (_dictionary)
                    {
                        _dictionary[key] = value;
                    }
                }
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            lock (LockForKey(key))
            {
                lock (_dictionary)
                {
                    if (_dictionary.ContainsKey(key)) return false;

                    _dictionary.Add(key, value);
                    return true;
                }
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            value = default(TValue);

            lock (LockForKey(key))
            {
                lock (_dictionary)
                {
                    if (!_dictionary.ContainsKey(key)) return false;

                    value = _dictionary[key];
                    _dictionary.Remove(key);
                    return true;
                }
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFunc)
        {
            TValue value;

            if (_dictionary.TryGetValue(key, out value)) return value;
            lock (LockForKey(key))
            {
                if (_dictionary.TryGetValue(key, out value)) return value;

                value = valueFunc(key);

                lock (_dictionary)
                {
                    _dictionary[key] = value;
                }

                return value;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_dictionary)
            {
                return _dictionary.TryGetValue(key, out value);
            }
        }

        public KeyValuePair<TKey, TValue>[] Where(Func<KeyValuePair<TKey, TValue>, bool> filter)
        {
            lock (_dictionary)
            {
                return _dictionary.Where(filter).ToArray();
            }
        }

        public IDictionary<TKey, TValue> ToDictionary()
        {
            lock (_dictionary)
            {
                return _dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        public void Clear()
        {
            lock (_dictionary)
            {
                _dictionary.Clear();
            }
        }

        private object LockForKey(TKey key)
        {
            object mutexForKey;

            if (_locks.TryGetValue(key, out mutexForKey)) return mutexForKey;
            lock (_locks)
            {
                if (_locks.TryGetValue(key, out mutexForKey)) return mutexForKey;

                mutexForKey = new object();
                _locks[key] = mutexForKey;

                return mutexForKey;
            }
        }
    }
}
