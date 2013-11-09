using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestResponseCorrelationWrapper<TResponse> : IRequestResponseCorrelationWrapper
    {
        private readonly DateTimeOffset _expiresAfter;
        private readonly ConcurrentBag<TResponse> _responses = new ConcurrentBag<TResponse>();

        public MulticastRequestResponseCorrelationWrapper(DateTimeOffset expiresAfter)
        {
            _expiresAfter = expiresAfter;
        }

        public Type ResponseType
        {
            get { return typeof (TResponse); }
        }

        public DateTimeOffset ExpiresAfter
        {
            get { return _expiresAfter; }
        }

        public void SetResponse(object response)
        {
            _responses.Add((TResponse) response);
        }

        public void Throw(string exceptionMessage, string exceptionStackTrace)
        {
            // don't care.
        }

        public IEnumerable<TResponse> WaitForResponses(TimeSpan timeout)
        {
            Thread.Sleep(timeout); // NOT for debugging - we actually just want to wait for however long we were told to
            var snapshot = _responses.ToArray(); // in case anyone arrives after we've expired
            return snapshot;
        }
    }
}