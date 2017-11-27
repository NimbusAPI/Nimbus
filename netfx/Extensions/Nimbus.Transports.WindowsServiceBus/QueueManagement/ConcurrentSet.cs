using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.Transports.WindowsServiceBus.QueueManagement
{
    internal class ConcurrentSet<T> : IEnumerable<T>
    {
        private readonly HashSet<T> _items = new HashSet<T>();
        private readonly object _mutex = new object();

        public ConcurrentSet()
        {
        }

        public ConcurrentSet(IEnumerable<T> items)
        {
            _items = new HashSet<T>(items);
        }

        public ConcurrentSet(params T[] items)
        {
            _items = new HashSet<T>(items);
        }

        public void Add(T item)
        {
            lock (_mutex)
            {
                _items.Add(item);
            }
        }

        public void Remove(T item)
        {
            lock (_mutex)
            {
                _items.Remove(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_mutex)
            {
                var clone = _items.ToArray();
                return clone.AsEnumerable().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}