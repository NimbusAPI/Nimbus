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

        public Task RemoveAllExistingNamespaceElements()
        {
            return Task.Run(() =>
                            {
                                var multiplexer = _multiplexerFunc();
                                var configuration = ConfigurationOptions.Parse(multiplexer.Configuration);

                                var database = configuration.DefaultDatabase ?? 0;
                                configuration.EndPoints
                                             .AsParallel()
                                             .Do(server => multiplexer.GetServer(server).FlushDatabase(database))
                                             .Done();
                            }).ConfigureAwaitFalse();
        }
    }
}