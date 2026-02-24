using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Transports.AMQP.DelayedDelivery
{
    internal class AMQPNamespaceCleanser : INamespaceCleanser
    {
        private readonly AMQPTransportConfiguration _configuration;
        private readonly ILogger _logger;

        public AMQPNamespaceCleanser(AMQPTransportConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task RemoveAllExistingNamespaceElements()
        {
            if (string.IsNullOrWhiteSpace(_configuration.ManagementUri))
            {
                _logger.Warn("AMQP namespace cleaning skipped: no ManagementUri configured. " +
                            "Use WithManagementUri() to enable cleanup via the Artemis Jolokia API.");
                return;
            }

            _logger.Info("Cleaning AMQP namespace via Artemis management API at {ManagementUri}", _configuration.ManagementUri);

            using var client = CreateHttpClient();

            var brokerMBean = await FindBrokerMBean(client);
            if (brokerMBean == null)
            {
                _logger.Warn("Could not find Artemis broker MBean. Skipping namespace cleanup.");
                return;
            }

            await DestroyQueues(client, brokerMBean);
            await DeleteAddresses(client, brokerMBean);
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_configuration.ManagementUri.TrimEnd('/') + "/") };

            if (!string.IsNullOrWhiteSpace(_configuration.Username))
            {
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_configuration.Username}:{_configuration.Password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }

            return client;
        }

        private async Task<string> FindBrokerMBean(HttpClient client)
        {
            var response = await client.GetAsync("console/jolokia/search/org.apache.activemq.artemis:broker=*");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var values = doc.RootElement.GetProperty("value");
            if (values.GetArrayLength() == 0) return null;

            var mbean = values[0].GetString();
            _logger.Debug("Found Artemis broker MBean: {MBean}", mbean);
            return mbean;
        }

        private async Task DestroyQueues(HttpClient client, string brokerMBean)
        {
            var queueNames = await GetNames(client, brokerMBean, "QueueNames");
            if (queueNames == null) return;

            foreach (var queue in queueNames.Where(ShouldClean))
            {
                try
                {
                    var encodedQueue = WebUtility.UrlEncode(queue);
                    var url = $"console/jolokia/exec/{WebUtility.UrlEncode(brokerMBean)}/destroyQueue(java.lang.String)/{encodedQueue}";
                    await client.GetAsync(url);
                    _logger.Debug("Destroyed queue: {Queue}", queue);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to destroy queue {Queue}", queue);
                }
            }
        }

        private async Task DeleteAddresses(HttpClient client, string brokerMBean)
        {
            var addressNames = await GetNames(client, brokerMBean, "AddressNames");
            if (addressNames == null) return;

            foreach (var address in addressNames.Where(ShouldClean))
            {
                try
                {
                    var encodedAddress = WebUtility.UrlEncode(address);
                    var url = $"console/jolokia/exec/{WebUtility.UrlEncode(brokerMBean)}/deleteAddress(java.lang.String)/{encodedAddress}";
                    await client.GetAsync(url);
                    _logger.Debug("Deleted address: {Address}", address);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to delete address {Address}", address);
                }
            }
        }

        private async Task<string[]> GetNames(HttpClient client, string brokerMBean, string attribute)
        {
            try
            {
                var url = $"console/jolokia/read/{WebUtility.UrlEncode(brokerMBean)}/{attribute}";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                var values = doc.RootElement.GetProperty("value");
                return values.EnumerateArray()
                             .Select(e => e.GetString())
                             .Where(s => s != null)
                             .ToArray();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to get {Attribute} from broker", attribute);
                return null;
            }
        }

        private bool ShouldClean(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            // Skip internal Artemis addresses/queues
            if (name.StartsWith("$") || name.StartsWith("activemq.") || name.StartsWith("notif")) return false;
            if (name.StartsWith("DLQ") || name.StartsWith("ExpiryQueue")) return false;

            return true;
        }
    }
}
