using System;
using System.Threading;

namespace Nimbus
{
    internal interface IRequestResponseWrapper
    {
        Semaphore Semaphore { get; }
        Type ResponseType { get; }
        void SetResponse(object response);
    }
}