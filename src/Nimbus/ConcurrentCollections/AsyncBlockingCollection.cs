using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.ConcurrentCollections
{
    internal class AsyncBlockingCollection<T>
    {
        readonly SemaphoreSlim _itemsSemaphore = new SemaphoreSlim(0, int.MaxValue);
        readonly ConcurrentQueue<T> _items = new ConcurrentQueue<T>();

        public async Task<T> Take(CancellationToken cancellationToken)
        {
            await _itemsSemaphore.WaitAsync(cancellationToken);
            try
            {
                T result;
                _items.TryDequeue(out result);
                return result;
            }
            finally
            {
                _itemsSemaphore.Release();
            }
        }

        public Task Add(T item)
        {
            return Task.Run(() =>
                            {
                                _items.Enqueue(item);
                                _itemsSemaphore.Release();
                            });
        }
    }
}