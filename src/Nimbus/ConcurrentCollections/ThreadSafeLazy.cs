using System;

namespace Nimbus.ConcurrentCollections
{
    public class ThreadSafeLazy<T>
    {
        private readonly Func<T> _valueFunc;
        private readonly object _mutex = new object();
        private T _value;

        public ThreadSafeLazy(Func<T> valueFunc)
        {
            _valueFunc = valueFunc;
        }

        public T Value
        {
            get
            {
                if (IsValueCreated) return _value;

                lock (_mutex)
                {
                    if (IsValueCreated) return _value;

                    _value = _valueFunc();
                    IsValueCreated = true;
                    return _value;
                }
            }
        }

        public bool IsValueCreated { get; private set; }
    }
}