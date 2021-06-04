namespace Nimbus.Transports.AzureServiceBus2.QueueManagement
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class ConcurrentSet<T> : IEnumerable<T>
    {
        private readonly HashSet<T> _items = new HashSet<T>();
        private readonly object _mutex = new object();

        public ConcurrentSet()
        {
        }

        public ConcurrentSet(IEnumerable<T> items)
        {
            this._items = new HashSet<T>(items);
        }

        public ConcurrentSet(params T[] items)
        {
            this._items = new HashSet<T>(items);
        }

        public void Add(T item)
        {
            lock (this._mutex)
            {
                this._items.Add(item);
            }
        }

        public void Remove(T item)
        {
            lock (this._mutex)
            {
                this._items.Remove(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this._mutex)
            {
                var clone = this._items.ToArray();
                return clone.AsEnumerable().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}