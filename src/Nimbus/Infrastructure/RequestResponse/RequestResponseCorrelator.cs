using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestResponseCorrelator
    {
        private const int _numMessagesBetweenScanningForExpiredWrappers = 1000;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Guid, IRequestResponseCorrelationWrapper> _requestWrappers = new ConcurrentDictionary<Guid, IRequestResponseCorrelationWrapper>();
        private int _messagesProcessed;
        private readonly object _mutex = new object();

        internal RequestResponseCorrelator(IClock clock, ILogger logger)
        {
            _clock = clock;
            _logger = logger;
        }

        internal RequestResponseCorrelationWrapper<TResponse> RecordRequest<TResponse>(Guid correlationId, DateTimeOffset expiresAfter)
        {
            RecordMessageProcessed();
            var wrapper = new RequestResponseCorrelationWrapper<TResponse>(expiresAfter);
            _requestWrappers[correlationId] = wrapper;
            return wrapper;
        }

        internal MulticastRequestResponseCorrelationWrapper<TResponse> RecordMulticastRequest<TResponse>(Guid correlationId, DateTimeOffset expiresAfter)
        {
            RecordMessageProcessed();
            var wrapper = new MulticastRequestResponseCorrelationWrapper<TResponse>(expiresAfter);
            _requestWrappers[correlationId] = wrapper;
            return wrapper;
        }

        internal IRequestResponseCorrelationWrapper TryGetWrapper(Guid correlationId)
        {
            IRequestResponseCorrelationWrapper wrapper;
            _requestWrappers.TryGetValue(correlationId, out wrapper);
            return wrapper;
        }

        internal void RemoveWrapper(Guid correlationId)
        {
            IRequestResponseCorrelationWrapper dummy;
            _requestWrappers.TryRemove(correlationId, out dummy);
        }

        private void RecordMessageProcessed()
        {
            var messageCount = Interlocked.Increment(ref _messagesProcessed);
            if (messageCount != _numMessagesBetweenScanningForExpiredWrappers) return;

            lock (_mutex)
            {
                _messagesProcessed %= _numMessagesBetweenScanningForExpiredWrappers;
            }

            RemoveExpiredWrappers();
        }

        private void RemoveExpiredWrappers()
        {
            Task.Run(() =>
            {
                var now = _clock.UtcNow;

                var toRemove = _requestWrappers
                    .Where(kvp => kvp.Value.ExpiresAfter >= now)
                    .ToArray();

                foreach (var kvp in toRemove)
                {
                    RemoveWrapper(kvp.Key);
                    _logger.Debug("Removing request {0} which timed out at {1}", kvp.Key, kvp.Value.ExpiresAfter);
                }
            });
        }
    }
}