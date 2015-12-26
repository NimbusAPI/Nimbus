using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Nimbus.ConcurrentCollections;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestResponseCorrelationWrapper<TResponse> : IRequestResponseCorrelationWrapper, IDisposable
    {
        private readonly AsyncBlockingCollection<TResponse> _responses = new AsyncBlockingCollection<TResponse>();

        public MulticastRequestResponseCorrelationWrapper(DateTimeOffset expiresAfter)
        {
            ExpiresAfter = expiresAfter;
        }

        public DateTimeOffset ExpiresAfter { get; }

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
                var response = _responses.TryTake(remainingTime, CancellationToken.None).Result;
                if (response != null) yield return response;
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