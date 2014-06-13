using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestResponseCorrelationWrapper<TResponse> : IRequestResponseCorrelationWrapper, IDisposable
    {
        private readonly DateTimeOffset _expiresAfter;
        private readonly BlockingCollection<TResponse> _responses = new BlockingCollection<TResponse>();

        public MulticastRequestResponseCorrelationWrapper(DateTimeOffset expiresAfter)
        {
            _expiresAfter = expiresAfter;
        }

        public DateTimeOffset ExpiresAfter
        {
            get { return _expiresAfter; }
        }

        public void Reply(object response)
        {
            _responses.Add((TResponse) response);
        }

        public void Throw(string exceptionMessage, string exceptionStackTrace)
        {
            // don't care.
        }

        public IEnumerable<TResponse> ReturnResponsesOpportunistically(TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                var remainingTime = timeout - sw.Elapsed;
                TResponse response;
                if (_responses.TryTake(out response, remainingTime)) yield return response;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _responses.Dispose();
        }
    }
}