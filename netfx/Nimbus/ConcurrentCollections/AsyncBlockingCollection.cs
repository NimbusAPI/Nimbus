using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Extensions;

namespace Nimbus.ConcurrentCollections
{
    internal class AsyncBlockingCollection<T> : IDisposable
    {
        private readonly SemaphoreSlim _itemsSemaphore = new SemaphoreSlim(0, int.MaxValue);
        private readonly ConcurrentQueue<T> _items = new ConcurrentQueue<T>();

        public Task<T> TryTake(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
                                  {
                                      try
                                      {
                                          await _itemsSemaphore.WaitAsync(timeout, cancellationToken);
                                          T result;
                                          _items.TryDequeue(out result);
                                          return result;
                                      }
                                      catch (OperationCanceledException)
                                      {
                                          return default(T);
                                      }
                                  },
                            cancellationToken).ConfigureAwaitFalse();
        }

        public Task<T> Take(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromTicks(int.MaxValue); // ConcurrentQueue has a different view on TimeSpan.MaxValue than the rest of the world :(
            return TryTake(timeout, cancellationToken);
        }

        public Task Add(T item)
        {
            return Task.Run(() =>
                            {
                                _items.Enqueue(item);
                                _itemsSemaphore.Release();
                            }).ConfigureAwaitFalse();
        }

        public void Dispose()
        {
            _itemsSemaphore.Dispose();
        }
    }
}