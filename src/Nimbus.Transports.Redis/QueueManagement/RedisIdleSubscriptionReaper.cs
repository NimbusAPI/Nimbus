using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Redis.MessageSendersAndReceivers;
using StackExchange.Redis;
using Timer = System.Timers.Timer;

namespace Nimbus.Transports.Redis.QueueManagement
{
    /// <summary>
    ///     Catches the case <see cref="RedisTopicSender" />'s prune-on-send can't reach: a topic nobody
    ///     is publishing to any more. Periodically scans every subscription Set for members whose
    ///     liveness key (see <see cref="RedisSubscriptionReceiver" />) has expired, and removes them along
    ///     with their now-orphaned message list.
    /// </summary>
    internal class RedisIdleSubscriptionReaper : IDisposable
    {
        private readonly Func<ConnectionMultiplexer> _multiplexerFunc;
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;
        private readonly ILogger _logger;
        private Timer _timer;

        public RedisIdleSubscriptionReaper(Func<ConnectionMultiplexer> multiplexerFunc, AutoDeleteOnIdleSetting autoDeleteOnIdle, ILogger logger)
        {
            _multiplexerFunc = multiplexerFunc;
            _autoDeleteOnIdle = autoDeleteOnIdle;
            _logger = logger;
        }

        public void Start()
        {
            if (_timer != null) return;

            // Same margin as the receiver's own heartbeat refresh - several sweeps happen inside a
            // single TTL window so one missed/slow sweep doesn't let a dead entry linger unreaped.
            var interval = TimeSpan.FromTicks(_autoDeleteOnIdle.Value.Ticks/4);

            _timer = new Timer(interval.TotalMilliseconds) {AutoReset = true};
            _timer.Elapsed += (s, e) => Task.Run(ReapOnce).ConfigureAwaitFalse();
            _timer.Start();
        }

        // internal rather than private so tests can trigger a single sweep synchronously instead of
        // waiting out the timer interval.
        internal Task ReapOnce()
        {
            return Task.Run(() =>
                            {
                                try
                                {
                                    var multiplexer = _multiplexerFunc();
                                    var database = multiplexer.GetDatabase();

                                    multiplexer.GetEndPoints()
                                               .AsParallel()
                                               .SelectMany(endpoint => multiplexer.GetServer(endpoint).Keys(pattern: $"{Subscription.SubscriptionsPrefix}.*"))
                                               .Do(subscribersRedisKey => ReapSubscribers(database, subscribersRedisKey))
                                               .Done();
                                }
                                catch (Exception exc)
                                {
                                    _logger.Warn(exc, "Idle subscription reaper sweep failed: {Message}", exc.Message);
                                }
                            }).ConfigureAwaitFalse();
        }

        private void ReapSubscribers(IDatabase database, RedisKey subscribersRedisKey)
        {
            var deadSubscribers = database.SetMembers(subscribersRedisKey)
                                          .Select(s => s.ToString())
                                          .Where(subscriberPath => !database.KeyExists(Subscription.SubscriberAliveRedisKeyFor(subscriberPath)))
                                          .ToArray();

            foreach (var subscriberPath in deadSubscribers)
            {
                _logger.Debug("Reaping idle subscription {SubscriberPath} from {SubscribersRedisKey}", subscriberPath, subscribersRedisKey);
                database.SetRemove(subscribersRedisKey, subscriberPath);
                database.KeyDelete(subscriberPath);
            }
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }
    }
}