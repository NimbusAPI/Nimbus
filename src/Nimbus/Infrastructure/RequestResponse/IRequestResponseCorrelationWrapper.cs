using System;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal interface IRequestResponseCorrelationWrapper
    {
        Type ResponseType { get; }
        void Reply(object response);
        void Throw(string exceptionMessage, string exceptionStackTrace);
        DateTimeOffset ExpiresAfter { get; }
    }
}