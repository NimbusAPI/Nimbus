using System;

namespace Nimbus.Infrastructure
{
    internal interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}