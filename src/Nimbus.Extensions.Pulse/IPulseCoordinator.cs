using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.Extensions.Pulse
{
    /// <summary>
    ///     Decides whether this instance gets to fire a particular scheduled occurrence.
    /// </summary>
    /// <remarks>
    ///     Implementations race every other instance of the same application for a given
    ///     (scheduleName, scheduledTimeUtc) pair. Exactly one racer should win. This is deliberately
    ///     per-occurrence rather than a long-lived leader lease: there are no heartbeats to tune, no
    ///     failure detection to get wrong, and no window after an instance dies during which nothing
    ///     fires.
    ///     A coordinator that can't reach its backing store should throw rather than guess. Pulse
    ///     treats a throw as "don't fire" and logs it, so an outage costs you occurrences rather than
    ///     firing them once per instance.
    /// </remarks>
    public interface IPulseCoordinator
    {
        Task<bool> TryClaim(string scheduleName, DateTimeOffset scheduledTimeUtc, CancellationToken cancellationToken);
    }
}
