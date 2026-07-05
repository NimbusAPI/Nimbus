using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;

namespace Nimbus.Transports.RabbitMQ.QueueManagement
{
    internal class RabbitMqNamespaceCleanser : INamespaceCleanser
    {
        private readonly RabbitMqTransportConfiguration _configuration;
        private readonly ILogger _logger;

        public RabbitMqNamespaceCleanser(RabbitMqTransportConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task RemoveAllExistingNamespaceElements()
        {
            var managementUri = _configuration.ManagementUri;
            if (string.IsNullOrWhiteSpace(managementUri))
            {
                _logger.Warn("RabbitMQ namespace cleaning skipped: no ManagementPort configured. " +
                             "Use WithManagementPort() to enable cleanup between tests.");
                return;
            }

            _logger.Info("Cleaning RabbitMQ namespace via management API at {ManagementUri}", managementUri);

            var vhost = Uri.EscapeDataString(_configuration.VirtualHost);

            using var client = CreateHttpClient(managementUri);

            await DeleteQueues(client, vhost);
            await DeleteExchanges(client, vhost);
        }

        private async Task DeleteQueues(HttpClient client, string vhost)
        {
            var queues = await GetNames(client, $"api/queues/{vhost}");
            if (queues == null) return;

            foreach (var name in queues)
            {
                if (IsSystemQueue(name)) continue;
                try
                {
                    await client.DeleteAsync($"api/queues/{vhost}/{Uri.EscapeDataString(name)}");
                    _logger.Debug("Deleted RabbitMQ queue: {Queue}", name);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to delete RabbitMQ queue {Queue}", name);
                }
            }
        }

        private async Task DeleteExchanges(HttpClient client, string vhost)
        {
            var exchanges = await GetNames(client, $"api/exchanges/{vhost}");
            if (exchanges == null) return;

            foreach (var name in exchanges)
            {
                if (IsSystemExchange(name)) continue;
                try
                {
                    await client.DeleteAsync($"api/exchanges/{vhost}/{Uri.EscapeDataString(name)}");
                    _logger.Debug("Deleted RabbitMQ exchange: {Exchange}", name);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to delete RabbitMQ exchange {Exchange}", name);
                }
            }
        }

        private async Task<string[]> GetNames(HttpClient client, string endpoint)
        {
            try
            {
                var response = await client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                var names = new List<string>();
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.TryGetProperty("name", out var nameProp))
                    {
                        var name = nameProp.GetString();
                        if (name != null) names.Add(name);
                    }
                }
                return names.ToArray();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to get items from management API endpoint {Endpoint}", endpoint);
                return null;
            }
        }

        private HttpClient CreateHttpClient(string baseUri)
        {
            var client = new HttpClient { BaseAddress = new Uri(baseUri.TrimEnd('/') + "/") };
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_configuration.Username}:{_configuration.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            return client;
        }

        private static bool IsSystemQueue(string name) =>
            string.IsNullOrEmpty(name) || name.StartsWith("amq.");

        private static bool IsSystemExchange(string name) =>
            string.IsNullOrEmpty(name) || name.StartsWith("amq.") || name == RabbitMqConnectionManager.DelayedExchangeName;
    }
}
