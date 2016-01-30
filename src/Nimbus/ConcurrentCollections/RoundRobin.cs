using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Extensions;

namespace Nimbus.ConcurrentCollections
{
    public class RoundRobin<T> : IDisposable
    {
        private readonly int _poolSize;
        private readonly Func<T> _createItem;
        private readonly Func<T, bool> _isBorked;
        private readonly Action<T> _disposeItem;

        private int _index;
        private readonly List<T> _items = new List<T>();
        private readonly object _itemsMutex = new object();
        private readonly object _repopulationMutex = new object();

        private bool _isDisposed;

        public RoundRobin(int poolSize, Func<T> createItem, Func<T, bool> isBorked, Action<T> disposeItem)
        {
            if (poolSize <= 0) throw new ArgumentException("poolSize");

            _poolSize = poolSize;
            _createItem = createItem;
            _isBorked = isBorked;
            _disposeItem = disposeItem;

            _index = 0;
        }

        public T GetNext()
        {
            if (_isDisposed) throw new ObjectDisposedException($"This {nameof(RoundRobin<T>)} has already been disposed.");

            while (true)
            {
                // we nest loop/lock/loop so that if we toss out all of our items we'll still have released the lock
                // sometimes so that someone else can populate the pool again.
                lock (_itemsMutex)
                {
                    while (_items.Any())
                    {
                        _index %= _items.Count;
                        var item = _items[_index];
                        _index++;

                        if (_isBorked(item))
                        {
                            _items.Remove(item);
                            _disposeItem(item);
                            TriggerRepopulation();
                            continue;
                        }

                        return item;
                    }
                }

                TriggerRepopulation().Wait();
            }
        }

        private Task TriggerRepopulation()
        {
            // slow (locks on the same mutex) but will work for now.
            return Task.Run(() =>
                            {
                                if (_isDisposed) return;

                                lock (_repopulationMutex)
                                {
                                    int numItemsRequired;

                                    lock (_itemsMutex)
                                    {
                                        numItemsRequired = _poolSize - _items.Count;
                                    }

                                    if (numItemsRequired <= 0) return;

                                    var newItems = Enumerable
                                        .Range(0, numItemsRequired)
                                        .AsParallel()
                                        .Select(i => _createItem())
                                        .ToArray();

                                    lock (_itemsMutex)
                                    {
                                        _items.AddRange(newItems);
                                    }
                                }
                            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_isDisposed) return;

            _isDisposed = true;

            lock (_itemsMutex)
            {
                _items
                    .Do(_disposeItem)
                    .Done();

                _items.Clear();
            }
        }
    }
}