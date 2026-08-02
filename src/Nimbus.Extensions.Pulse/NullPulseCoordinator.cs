using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.Extensions.Pulse
{
    /// <summary>
    ///     The default coordinator. Every instance wins every occurrence, so a schedule fires once per
    ///     running instance of your application. Fine for a single instance; configure a real
    ///     coordinator before you scale out.
    /// </summary>
    public class NullPulseCoordinator : IPulseCoordinator
    {
        public Task<bool> TryClaim(string scheduleName, DateTimeOffset scheduledTimeUtc, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
