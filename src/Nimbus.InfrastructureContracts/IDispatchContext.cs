using System;

namespace Nimbus.InfrastructureContracts
{
    public interface IDispatchContext
    {
        Guid DispatchId { get; }
        Guid? ResultOfMessageId { get; }
        Guid CorrelationId { get; }
    }
}