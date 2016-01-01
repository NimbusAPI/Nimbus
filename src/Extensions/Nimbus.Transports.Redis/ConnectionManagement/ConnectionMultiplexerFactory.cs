using Nimbus.Configuration.Settings;
using Nimbus.Transports.Redis.Configuration;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.ConnectionManagement
{
    internal class ConnectionMultiplexerFactory
    {
        private readonly ApplicationNameSetting _applicationName;
        private readonly InstanceNameSetting _instanceName;
        private readonly RedisConnectionString _connectionString;

        public ConnectionMultiplexerFactory(ApplicationNameSetting applicationName, InstanceNameSetting instanceName, RedisConnectionString connectionString)
        {
            _applicationName = applicationName;
            _instanceName = instanceName;
            _connectionString = connectionString;
        }

        public ConnectionMultiplexer Create()
        {
            var configuration = ConfigurationOptions.Parse(_connectionString);
            configuration.AbortOnConnectFail = false;
            configuration.AllowAdmin = true;
            configuration.ClientName = $"{_applicationName}.{_instanceName}";

            var multiplexer = ConnectionMultiplexer.Connect(configuration);
            multiplexer.PreserveAsyncOrder = false;
            multiplexer.IncludeDetailInExceptions = true;
            return multiplexer;
        }
    }
}