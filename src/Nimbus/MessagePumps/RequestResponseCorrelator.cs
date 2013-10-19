using System;
using System.Collections.Concurrent;

namespace Nimbus.MessagePumps
{
    public class RequestResponseCorrelator
    {
        private readonly ConcurrentDictionary<Guid, IRequestResponseCorrelationWrapper> _requestWrappers = new ConcurrentDictionary<Guid, IRequestResponseCorrelationWrapper>();

        internal RequestResponseCorrelationWrapper<TResponse> RecordRequest<TResponse>(Guid correlationId)
        {
            var wrapper = new RequestResponseCorrelationWrapper<TResponse>();
            _requestWrappers[correlationId] = wrapper;
            return wrapper;
        }

        internal IRequestResponseCorrelationWrapper TryGetWrapper(Guid correlationId)
        {
            IRequestResponseCorrelationWrapper wrapper;
            return _requestWrappers.TryRemove(correlationId, out wrapper) ? wrapper : null;
        }
    }
}