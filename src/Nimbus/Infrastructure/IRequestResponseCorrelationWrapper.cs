using System;

namespace Nimbus.Infrastructure
{
    internal interface IRequestResponseCorrelationWrapper
    {
        Type ResponseType { get; }
        void SetResponse(object response);
    }
}