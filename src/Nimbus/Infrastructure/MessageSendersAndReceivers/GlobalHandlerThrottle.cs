using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    public class GlobalHandlerThrottle : IGlobalHandlerThrottle, IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public GlobalHandlerThrottle(GlobalConcurrentHandlerLimitSetting globalConcurrentHandlerLimit)
        {
            _semaphore = new SemaphoreSlim(globalConcurrentHandlerLimit, globalConcurrentHandlerLimit);
        }

        public Task Wait(CancellationToken ct)
        {
            return _semaphore.WaitAsync(ct);
        }

        public void Release()
        {
            _semaphore.Release();
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}