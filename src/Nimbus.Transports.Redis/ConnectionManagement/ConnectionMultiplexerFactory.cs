using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Redis.Configuration;
using StackExchange.Redis;

namespace Nimbus.Transports.Redis.ConnectionManagement
{
    internal class ConnectionMultiplexerFactory
    {
        private readonly ApplicationNameSetting _applicationName;
        private readonly InstanceNameSetting _instanceName;
        private readonly RedisConnectionString _connectionString;
        private readonly DefaultTimeoutSetting _timeout;
        private readonly ILogger _logger;

        public ConnectionMultiplexerFactory(ApplicationNameSetting applicationName,
                                            InstanceNameSetting instanceName,
                                            RedisConnectionString connectionString,
                                            DefaultTimeoutSetting timeout,
                                            ILogger logger)
        {
            _applicationName = applicationName;
            _instanceName = instanceName;
            _connectionString = connectionString;
            _timeout = timeout;
            _logger = logger;
        }

        public async Task TestConnection()
        {
            var configuration = ConstructConfigurationOptions();
            configuration.AbortOnConnectFail = true;
            configuration.ConnectTimeout = (int) _timeout.Value.TotalMilliseconds;

            using (var multiplexer = await ConnectionMultiplexer.ConnectAsync(configuration))
            {
                var status = multiplexer.GetStatus();
                _logger.Debug("Redis connection status: {RedisConnectionStatus}", status);
            }
        }

        public ConnectionMultiplexer Create()
        {
            var configuration = ConstructConfigurationOptions();

            var multiplexer = ConnectionMultiplexer.Connect(configuration);
            multiplexer.PreserveAsyncOrder = false;
            multiplexer.IncludeDetailInExceptions = true;
            multiplexer.ErrorMessage += OnErrorMessage;
            multiplexer.InternalError += OnInternalError;
            return multiplexer;
        }

        private void OnInternalError(object sender, InternalErrorEventArgs e)
        {
            _logger.Warn(e.Exception, "An internal error occurred in the StackExchange.Redis library ({Origin})", e.Origin);
        }

        private void OnErrorMessage(object sender, RedisErrorEventArgs e)
        {
            _logger.Warn("An error message was received from the Redis server at {Endpoint}: {Message}", e.EndPoint, e.Message);
        }

        private ConfigurationOptions ConstructConfigurationOptions()
        {
            var configuration = ConfigurationOptions.Parse(_connectionString);
            configuration.AbortOnConnectFail = false;
            configuration.AllowAdmin = true;
            configuration.ClientName = $"{_applicationName}.{_instanceName}";
            return configuration;
        }
    }
}