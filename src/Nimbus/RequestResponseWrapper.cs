using System;
using System.Threading;

namespace Nimbus
{
    internal class RequestResponseWrapper<TResponse> : IRequestResponseWrapper
    {
        private readonly Semaphore _semaphore;
        private TResponse _response;

        public RequestResponseWrapper()
        {
            _semaphore = new Semaphore(0, 1);
        }

        public Semaphore Semaphore
        {
            get { return _semaphore; }
        }

        public Type ResponseType
        {
            get { return typeof (TResponse); }
        }

        public TResponse Response
        {
            get { return _response; }
        }

        public void SetResponse(object response)
        {
            _response = (TResponse) response;
        }
    }
}