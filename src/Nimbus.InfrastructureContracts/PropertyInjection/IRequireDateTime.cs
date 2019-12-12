using System;

namespace Nimbus.InfrastructureContracts.PropertyInjection
{
    public interface IRequireDateTime
    {
        Func<DateTimeOffset> UtcNow { get; set; }
    }
}