using System;
using System.Threading;
using Nimbus.Exceptions;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestResponseCorrelationWrapper<TResponse> : IRequestResponseCorrelationWrapper
    {
        private readonly DateTimeOffset _expiresAfter;
        private readonly Semaphore _semaphore;
        private bool _requestWasSuccessful;
        private TResponse _response;
        private string _exceptionMessage;
        private string _exceptionStackTrace;

        public RequestResponseCorrelationWrapper(DateTimeOffset expiresAfter)
        {
            _expiresAfter = expiresAfter;
            _semaphore = new Semaphore(0, int.MaxValue);
        }

        public Type ResponseType
        {
            get { return typeof (TResponse); }
        }

        public void Reply(object response)
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

        public DateTimeOffset ExpiresAfter
        {
            get { return _expiresAfter; }
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