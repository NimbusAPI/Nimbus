using System;
using System.Threading;

namespace Nimbus
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
            _semaphore.WaitOne();

            return _response;
        }
    }
}