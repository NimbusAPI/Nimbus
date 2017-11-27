using System;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal interface IRequestResponseCorrelationWrapper
    {
        Task Reply(object response);
        Task Throw(string exceptionMessage, string exceptionStackTrace);
        DateTimeOffset ExpiresAfter { get; }
    }
}