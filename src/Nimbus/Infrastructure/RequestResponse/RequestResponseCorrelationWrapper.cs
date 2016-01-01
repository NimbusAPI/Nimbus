using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Exceptions;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestResponseCorrelationWrapper<TResponse> : IRequestResponseCorrelationWrapper, IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _requestWasSuccessful;
        private TResponse _response;
        private string _exceptionMessage;
        private string _exceptionStackTrace;

        public RequestResponseCorrelationWrapper(DateTimeOffset expiresAfter)
        {
            ExpiresAfter = expiresAfter;
            _semaphore = new SemaphoreSlim(0, int.MaxValue);
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

        public DateTimeOffset ExpiresAfter { get; }

        public Task<TResponse> WaitForResponse()
        {
            return WaitForResponse(TimeSpan.MaxValue);
        }

        public async Task<TResponse> WaitForResponse(TimeSpan timeout)
        {
            var responseReceivedInTime = await _semaphore.WaitAsync(timeout);

            if (!responseReceivedInTime)
            {
                throw new TimeoutException("No response was received within the configured timeout.")
                    .WithData("ExpectedResponseType", typeof (TResponse))
                    .WithData("Timeout", timeout);
            }

            if (!_requestWasSuccessful)
            {
                throw new RequestFailedException(_exceptionMessage, _exceptionStackTrace);
            }

            return _response;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _semaphore.Dispose();
        }
    }
}