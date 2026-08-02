using System;
using Nimbus.Extensions.Pulse.Configuration;

namespace Nimbus.Extensions.Pulse.Redis.Configuration
{
    public static class PulseRedisCoordinatorExtensions
    {
        /// <summary>
        ///     The default claim lifetime. Claims only need to outlive the spread between instances
        ///     reaching the same occurrence — clock skew plus scheduling lag — after which Redis expires
        ///     them on its own. An hour is generous for both and costs one small key per occurrence.
        /// </summary>
        private static readonly TimeSpan _defaultClaimTimeToLive = TimeSpan.FromHours(1);

        /// <summary>
        ///     Uses Redis to ensure each scheduled occurrence fires on exactly one instance of this
        ///     application. Independent of the bus transport — the bus can be running on Rabbit, Azure
        ///     Service Bus or anything else.
        /// </summary>
        /// <param name="claimTimeToLive">
        ///     How long a claim is remembered. Must comfortably exceed the worst-case clock skew between
        ///     your instances; an instance arriving after the claim expires will fire the occurrence again.
        /// </param>
        public static PulseEnabledBusBuilderConfiguration WithRedisCoordinator(
            this PulseEnabledBusBuilderConfiguration config,
            string connectionString,
            TimeSpan? claimTimeToLive = null)
        {
            return config.WithCoordinator(new RedisPulseCoordinator(connectionString,
                                                                    config.ApplicationName,
                                                                    claimTimeToLive ?? _defaultClaimTimeToLive,
                                                                    config.Logger));
        }
    }
}
