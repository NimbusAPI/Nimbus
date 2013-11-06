using System;
using System.Threading;
using Nimbus.Exceptions;

namespace Nimbus.Infrastructure
{
    internal class RequestResponseCorrelationWrapper<TResponse> : IRequestResponseCorrelationWrapper
    {
        private readonly Semaphore _semaphore;
        private bool _requestWasSuccessful;
        private TResponse _response;
        private string _exceptionMessage;
        private string _exceptionStackTrace;

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
            _requestWasSuccessful = true;
            _response = (TResponse) response;
            _semaphore.Release();
        }

        public void Throw(string exceptionMessage, string exceptionStackTrace)
        {
            _requestWasSuccessful = false;
            _exceptionStackTrace = exceptionStackTrace;
            _exceptionMessage = exceptionMessage;
            _semaphore.Release();
        }

        public TResponse WaitForResponse()
        {
            return WaitForResponse(TimeSpan.MaxValue);
        }

        public TResponse WaitForResponse(TimeSpan timeout)
        {
            if (!_semaphore.WaitOne(timeout)) throw new TimeoutException("No response was received from the bus within the configured timeout.");
            if (!_requestWasSuccessful) throw new RequestFailedException(_exceptionMessage, _exceptionStackTrace);
            return _response;
        }
    }
}