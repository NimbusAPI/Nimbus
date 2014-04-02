using System;

namespace Nimbus.Infrastructure
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}