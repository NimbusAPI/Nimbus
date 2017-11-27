using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Transports.Redis.MessageSendersAndReceivers;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.QueueManagement
{
    public class NamespaceCleanser : INamespaceCleanser
    {
        private readonly GlobalPrefixSetting _globalPrefix;
        private readonly Func<ConnectionMultiplexer> _multiplexerFunc;
        private readonly ILogger _logger;

        public NamespaceCleanser(GlobalPrefixSetting globalPrefix, Func<ConnectionMultiplexer> multiplexerFunc, ILogger logger)
        {
            _globalPrefix = globalPrefix;
            _multiplexerFunc = multiplexerFunc;
            _logger = logger;
        }

        public Task RemoveAllExistingNamespaceElements()
        {
            return Task.Run(() =>
                            {
                                var multiplexer = _multiplexerFunc();
                                var configuration = ConfigurationOptions.Parse(multiplexer.Configuration);
                                var databaseNumber = configuration.DefaultDatabase ?? 0;
                                var database = multiplexer.GetDatabase(databaseNumber);

                                configuration.EndPoints
                                             .AsParallel()
                                             .SelectMany(endpoint => FetchAllKeys(multiplexer, endpoint))
                                             .Do(redisKey => DeleteKey(database, redisKey))
                                             .Done();
                            }).ConfigureAwaitFalse();
        }

        private static IEnumerable<RedisKey> FetchAllKeys(ConnectionMultiplexer multiplexer, EndPoint endpoint)
        {
            var server = multiplexer.GetServer(endpoint);

            var allKeys = server.Keys()
                                .ToArray();

            return allKeys;
        }

        private void DeleteKey(IDatabase database, RedisKey redisKey)
        {
            var key = (string) redisKey;
            if (key.StartsWith(_globalPrefix.Value)) ActuallyDeleteKey(database, redisKey);
            if (key.StartsWith($"{Subscription.SubscriptionsPrefix}.{_globalPrefix.Value}")) ActuallyDeleteKey(database, redisKey);
        }

        private void ActuallyDeleteKey(IDatabase database, RedisKey key)
        {
            _logger.Debug("Deleting {RedisKey}", key);
            database.KeyDelete(key);
        }
    }
}