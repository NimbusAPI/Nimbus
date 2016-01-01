using System;
using System.Threading.Tasks;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.DelayedDelivery
{
    internal class RedisDelayedDeliveryService : IDelayedDeliveryService
    {
        private readonly IClock _clock;
        private readonly Func<IDatabase> _databaseFunc;
        private readonly ISerializer _serializer;

        public RedisDelayedDeliveryService(IClock clock, Func<IDatabase> databaseFunc, ISerializer serializer)
        {
            _clock = clock;
            _databaseFunc = databaseFunc;
            _serializer = serializer;
        }

        public Task DeliverAfter(NimbusMessage message, DateTimeOffset deliveryTime)
        {
            // Deliberately not awaiting this task. We want it to run in the background.
            Task.Run(async () =>
                           {
                               var delay = deliveryTime.Subtract(_clock.UtcNow);
                               if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
                               await Task.Delay(delay);

                               var database = _databaseFunc();
                               var serialized = _serializer.Serialize(message);
                               await database.ListRightPushAsync(message.To, serialized);
                           }).ConfigureAwaitFalse();

            return Task.Delay(0);
        }
    }
}