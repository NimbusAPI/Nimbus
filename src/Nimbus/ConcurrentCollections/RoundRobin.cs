using System;

namespace Nimbus.ConcurrentCollections
{
    public class RoundRobin<T>
    {
        private readonly T[] _items;
        private int _index;

        public RoundRobin(T[] items)
        {
            if (items.Length == 0) throw new ArgumentException("items");

            _items = items;
            _index = -1;
        }

        public T GetNext()
        {
            lock (_items)
            {
                _index++;
                _index %= _items.Length;
                return _items[_index];
            }
        }
    }
}