using System;
using System.Linq;
using Apache.NMS;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AMQP.ConnectionManagement
{
    internal class NmsConnectionFactory
    {
        private readonly AMQPTransportConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IConnectionFactory _factory;

        public NmsConnectionFactory(AMQPTransportConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
            _factory = new Apache.NMS.AMQP.NmsConnectionFactory(BuildConnectionUri());
        }

        public IConnection CreateConnection()
        {
            _logger.Debug("Creating new NMS connection to {BrokerUri}", _configuration.BrokerUri);

            var connection = string.IsNullOrWhiteSpace(_configuration.Username)
                ? _factory.CreateConnection()
                : _factory.CreateConnection(_configuration.Username, _configuration.Password);

            if (!string.IsNullOrWhiteSpace(_configuration.ClientId))
            {
                connection.ClientId = _configuration.ClientId;
            }

            connection.ExceptionListener += OnConnectionException;
            connection.Start();

            _logger.Info("Created and started NMS connection to {BrokerUri}", _configuration.BrokerUri);
            return connection;
        }

        private void OnConnectionException(Exception exception)
        {
            _logger.Error(exception, "NMS connection exception occurred");
        }

        private string BuildConnectionUri()
        {
            if (_configuration.FailoverUris == null || !_configuration.FailoverUris.Any())
            {
                return _configuration.BrokerUri;
            }

            // Build failover URI: failover:(uri1,uri2,uri3)
            var uris = new[] { _configuration.BrokerUri }.Concat(_configuration.FailoverUris);
            var uriList = string.Join(",", uris);
            return $"failover:({uriList})";
        }
    }
}
