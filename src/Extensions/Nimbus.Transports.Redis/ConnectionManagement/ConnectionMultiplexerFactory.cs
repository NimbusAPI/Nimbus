using Nimbus.Transports.Redis.Configuration;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.ConnectionManagement
{
    internal class ConnectionMultiplexerFactory
    {
        private readonly RedisConnectionString _connectionString;

        public ConnectionMultiplexerFactory(RedisConnectionString connectionString)
        {
            _connectionString = connectionString;
        }

        public ConnectionMultiplexer Create()
        {
            var multiplexer = ConnectionMultiplexer.Connect(_connectionString);
            multiplexer.PreserveAsyncOrder = false;
            multiplexer.IncludeDetailInExceptions = true;
            return multiplexer;
        }
    }
}