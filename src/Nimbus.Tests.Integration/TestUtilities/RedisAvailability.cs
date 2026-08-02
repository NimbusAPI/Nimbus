using System;
using Nimbus.ConcurrentCollections;
using Nimbus.Tests.Integration.Configuration;
using StackExchange.Redis;

namespace Nimbus.Tests.Integration.TestUtilities
{
    /// <summary>
    ///     Probes once per test run for a reachable Redis server, so that tests needing one can be
    ///     skipped visibly rather than failing with a connection error.
    /// </summary>
    public static class RedisAvailability
    {
        private static readonly ThreadSafeLazy<bool> _isReachable = new ThreadSafeLazy<bool>(Probe);

        public static string ConnectionString => AppSettingsLoader.Settings.Transports.Redis.ConnectionString;

        public static bool IsReachable => _isReachable.Value;

        private static bool Probe()
        {
            try
            {
                var options = ConfigurationOptions.Parse(ConnectionString);
                options.AbortOnConnectFail = true;
                options.ConnectTimeout = 2000;
                options.ConnectRetry = 1;

                using (var multiplexer = ConnectionMultiplexer.Connect(options))
                {
                    return multiplexer.IsConnected;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Redis is not reachable at '{ConnectionString}': {exc.Message}");
                return false;
            }
        }
    }
}
