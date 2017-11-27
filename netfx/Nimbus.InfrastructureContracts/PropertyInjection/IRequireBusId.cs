using System;

namespace Nimbus.PropertyInjection
{
    /// <summary>
    /// Implement this interface to have the unique InstanceId of the current Bus instance injected into your handler.
    /// </summary>
    public interface IRequireBusId
    {
        Guid BusId { get; set; }
    }
}