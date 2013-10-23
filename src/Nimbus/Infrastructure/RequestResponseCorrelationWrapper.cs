using System;
using System.Threading;

namespace Nimbus.Infrastructure
{
    internal class RequestResponseCorrelationWrapper<TResponse> : IRequestResponseCorrelationWrapper
    {
        private readonly Semaphore _semaphore;
        private TResponse _response;

        public RequestResponseCorrelationWrapper()
        {
            _semaphore = new Semaphore(0, 1);
        }

        public Type ResponseType
        {
            get { return typeof (TResponse); }
        }

        public void SetResponse(object response)
        {
            _response = (TResponse) response;
            _semaphore.Release();
        }

        public TResponse WaitForResponse()
        {
            return WaitForResponse(TimeSpan.MaxValue);
        }

        public TResponse WaitForResponse(TimeSpan timeout)
        {
            if (!_semaphore.WaitOne(timeout)) throw new TimeoutException("No response was received from the bus within the configured timeout.");

            return _response;
        }
    }
}