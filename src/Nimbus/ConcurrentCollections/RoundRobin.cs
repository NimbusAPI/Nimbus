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

        private void ReturnToPool(Wrapper wrapper)
        {
            throw new NotImplementedException();
        }


        public class Wrapper: IDisposable
        {
            private readonly T _value;
            private readonly RoundRobin<T> _owner;
            private bool _isFaulted;

            public Wrapper(T value, RoundRobin<T> owner )
            {
                _value = value;
                _owner = owner;
            }

            public T Value
            {
                get { return _value; }
            }

            public bool IsFaulted
            {
                get { return _isFaulted; }
            }

            public void MarkAsFaulted()
            {
                _isFaulted = true;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposing) return;

                _owner.ReturnToPool(this);
            }
        }

    }

}