using System;

namespace Nimbus.ConcurrentCollections
{
    public class ThreadSafeLazy<T>
    {
        private readonly Func<T> _valueFunc;
        private readonly object _mutex = new object();
        private T _value;
        private bool _isValueCreated;
        private bool _isValueFaulted;
        private Exception _fault;

        public ThreadSafeLazy(Func<T> valueFunc)
        {
            _valueFunc = valueFunc;
        }

        public T Value
        {
            get
            {
                if (_isValueFaulted) throw _fault;

                if (_isValueCreated) return _value;

                lock (_mutex)
                {
                    if (_isValueFaulted) throw _fault;

                    if (_isValueCreated) return _value;

                    try
                    {
                        _value = _valueFunc();

                        _isValueCreated = true;
                    }
                    catch (Exception e)
                    {
                        _fault = e;

                        _isValueFaulted = true;

                        throw;
                    }

                    return _value;
                }
            }
        }

        public bool IsValueCreated
        {
            get { return _isValueCreated; }
        }

        public bool IsValueFaulted
        {
            get { return _isValueFaulted; }
        }
    }
}