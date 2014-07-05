using System;

namespace Nimbus
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}