using System;
using System.Net;
using System.Threading.Tasks;

namespace Nimbus.LargeMessages.Azure.Infrastructure.Http
{
    internal class AzureBlobStorageHttpClient : IAzureBlobStorageHttpClient
    {
        private readonly IUriFormatter _uriFormatter;
        private readonly ILogger _logger;

        public AzureBlobStorageHttpClient(IUriFormatter uriFormatter, ILogger logger)
        {
            _uriFormatter = uriFormatter;
            _logger = logger;
        }

        public async Task Upload(string storageKey, byte[] bytes)
        {
            if (storageKey == null) throw new ArgumentNullException("storageKey");
            if (bytes == null) throw new ArgumentNullException("bytes");

            var uri = _uriFormatter.FormatUri(storageKey);
            _logger.Debug("Writing blob {0} to {1}", storageKey, uri);

            var request = BuildWebRequest(uri, bytes);
            using (var requestStream = request.GetRequestStream())
            {
                await requestStream.WriteAsync(bytes, 0, bytes.Length);
            }
            using (request.GetResponse()) { } // throws if there was an error
        }

        public async Task Delete(string storageKey)
        {
            if (storageKey == null) throw new ArgumentNullException("storageKey");

            var uri = _uriFormatter.FormatUri(storageKey);
            _logger.Debug("Deleting blob {0} from {1}", storageKey, uri);
            using (var webClient = new WebClient())
            {
                await webClient.UploadStringTaskAsync(uri, "DELETE", "");
            }
        }

        public async Task<byte[]> Retrieve(string storageKey)
        {
            var uri = _uriFormatter.FormatUri(storageKey);
            _logger.Debug("Reading blob {0} from {1}", storageKey, uri);
            using (var webClient = new WebClient())
            {
                var result = await webClient.DownloadDataTaskAsync(uri);
                return result;
            }
        }

        private WebRequest BuildWebRequest(Uri uri, byte[] content)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (content == null) throw new ArgumentNullException("content");

            var request = WebRequest.Create(uri);
            request.Method = "PUT";
            request.Headers.Add("x-ms-blob-type", "BlockBlob");
            request.ContentLength = content.Length;
            return request;
        }
    }
}