using System;
using System.Collections.Generic;

namespace Nimbus.MessagePumps
{
    public class RequestResponseCorrelator
    {
        private readonly IDictionary<Guid, IRequestResponseCorrelationWrapper> _requestWrappers = new Dictionary<Guid, IRequestResponseCorrelationWrapper>();

        internal RequestResponseCorrelationWrapper<TResponse> RecordRequest<TResponse>(Guid correlationId)
        {
            var wrapper = new RequestResponseCorrelationWrapper<TResponse>();
            _requestWrappers[correlationId] = wrapper;
            return wrapper;
        }

        internal IRequestResponseCorrelationWrapper TryGetWrapper(Guid correlationId)
        {
            IRequestResponseCorrelationWrapper wrapper;
            return _requestWrappers.TryGetValue(correlationId, out wrapper) ? wrapper : null;
        }
    }
}