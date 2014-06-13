using System;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal interface IRequestResponseCorrelationWrapper
    {
        void Reply(object response);
        void Throw(string exceptionMessage, string exceptionStackTrace);
        DateTimeOffset ExpiresAfter { get; }
    }
}