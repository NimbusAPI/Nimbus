using System;

namespace Nimbus
{
    internal interface IRequestResponseCorrelationWrapper
    {
        Type ResponseType { get; }
        void SetResponse(object response);
    }
}