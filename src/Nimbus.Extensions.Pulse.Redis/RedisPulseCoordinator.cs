using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.ConcurrentCollections;
using Nimbus.InfrastructureContracts;
using StackExchange.Redis;

namespace Nimbus.Extensions.Pulse.Redis
{
    /// <summary>
    ///     Claims occurrences with a single <c>SET key owner NX PX ttl</c>. Redis executes commands one
    ///     at a time, so exactly one instance can win a given key — no lease renewal, no heartbeats, and
    ///     nothing to tune for failure detection.
    /// </summary>
    public class RedisPulseCoordinator : IPulseCoordinator, IDisposable
    {
        private readonly string _keyPrefix;
        private readonly TimeSpan _claimTimeToLive;
        private readonly ILogger _logger;
        private readonly string _owner;
        private readonly ThreadSafeLazy<ConnectionMultiplexer> _multiplexer;

        public RedisPulseCoordinator(string connectionString, string applicationName, TimeSpan claimTimeToLive, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("A Redis connection string is required.", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(applicationName)) throw new ArgumentException("An application name is required.", nameof(applicationName));
            if (claimTimeToLive <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(claimTimeToLive), "Claim time-to-live must be positive.");

            _keyPrefix = $"pulse:{applicationName}";
            _claimTimeToLive = claimTimeToLive;
            _logger = logger;
            _owner = $"{applicationName}/{Environment.MachineName}/{Guid.NewGuid():N}";

            _multiplexer = new ThreadSafeLazy<ConnectionMultiplexer>(() =>
                                                                    {
                                                                        var options = ConfigurationOptions.Parse(connectionString);
                                                                        options.AbortOnConnectFail = false;
                                                                        options.ClientName = $"{applicationName}.Pulse";
                                                                        return ConnectionMultiplexer.Connect(options);
                                                                    });
        }

        public async Task<bool> TryClaim(string scheduleName, DateTimeOffset scheduledTimeUtc, CancellationToken cancellationToken)
        {
            var key = KeyFor(scheduleName, scheduledTimeUtc);

            // Deliberately unguarded: if Redis is unreachable this throws, and Pulse skips the
            // occurrence rather than firing it on every instance.
            var won = await _multiplexer.Value
                                        .GetDatabase()
                                        .StringSetAsync(key, _owner, _claimTimeToLive, When.NotExists)
                                        .ConfigureAwait(false);

            if (won)
                _logger.Debug("Pulse claimed occurrence {PulseClaimKey} for {PulseClaimOwner}.", key, _owner);

            return won;
        }

        private string KeyFor(string scheduleName, DateTimeOffset scheduledTimeUtc)
        {
            // UtcTicks rather than a formatted timestamp so that two instances in different local time
            // zones produce byte-identical keys for the same occurrence.
            return $"{_keyPrefix}:{scheduleName}:{scheduledTimeUtc.UtcTicks}";
        }

        public void Dispose()
        {
            if (_multiplexer.IsValueCreated) _multiplexer.Value.Dispose();
        }
    }
}
