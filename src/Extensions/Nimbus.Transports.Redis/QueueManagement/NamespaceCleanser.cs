using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Extensions;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.QueueManagement
{
    public class NamespaceCleanser : INamespaceCleanser
    {
        private readonly Func<ConnectionMultiplexer> _multiplexerFunc;

        public NamespaceCleanser(Func<ConnectionMultiplexer> multiplexerFunc)
        {
            _multiplexerFunc = multiplexerFunc;
        }

        public async Task RemoveAllExistingNamespaceElements()
        {
            var multiplexer = _multiplexerFunc();
            var configuration = ConfigurationOptions.Parse(multiplexer.Configuration);

            var database = configuration.DefaultDatabase ?? 0;
            await configuration.EndPoints
                               .Select(server => multiplexer.GetServer(server).FlushDatabaseAsync(database))
                               .WhenAll();
        }
    }
}