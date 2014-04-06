using System;

namespace Nimbus.ConcurrentCollections
{
    public class ThreadSafeLazy<T>
    {
        private readonly Func<T> _valueFunc;
        private readonly object _mutex = new object();
        private T _value;
        private bool _isValueCreated;

        public ThreadSafeLazy(Func<T> valueFunc)
        {
            _valueFunc = valueFunc;
        }

        public T Value
        {
            get
            {
                if (_isValueCreated) return _value;

                lock (_mutex)
                {
                    if (_isValueCreated) return _value;

                    _value = _valueFunc();
                    _isValueCreated = true;
                    return _value;
                }
            }
        }

        public bool IsValueCreated
        {
            get { return _isValueCreated; }
        }
    }
}