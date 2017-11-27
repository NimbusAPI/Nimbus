using System;

namespace Nimbus
{
    public interface IDispatchContext
    {
        Guid DispatchId { get; }
        Guid? ResultOfMessageId { get; }
        Guid CorrelationId { get; }
    }
}