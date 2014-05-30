using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Nimbus.LargeMessages.Azure.Infrastructure.RestApi
{
    internal class RestApiHelper : IRestApiHelper
    {
        private readonly IUriFormatter _uriFormatter;
        private readonly ILogger _logger;

        public RestApiHelper(IUriFormatter uriFormatter, ILogger logger)
        {
            _uriFormatter = uriFormatter;
            _logger = logger;
        }

        public async Task Upload(string storageKey, byte[] bytes)
        {
            if (storageKey == null) throw new ArgumentNullException("storageKey");
            if (bytes == null) throw new ArgumentNullException("bytes");

            string uri = _uriFormatter.FormatUri(storageKey);
            _logger.Debug("Writing blob {0} to {1}", storageKey, uri);

            WebRequest request = CreateRequest(uri, bytes);
            using (Stream requestStream = request.GetRequestStream())
            {
                await requestStream.WriteAsync(bytes, 0, bytes.Length);
            }
            using (WebResponse resp = request.GetResponse()) { } // throws if there was an error
        }

        public async Task Delete(string storageKey)
        {
            if (storageKey == null) throw new ArgumentNullException("storageKey");

            string uri = _uriFormatter.FormatUri(storageKey);
            _logger.Debug("Deleting blob {0} from {1}", storageKey, uri);
            using (var webClient = new WebClient())
            {
                await webClient.UploadStringTaskAsync(uri, "DELETE", "");
            }
        }

        public async Task<byte[]> Retrieve(string storageKey)
        {
            var url = _uriFormatter.FormatUri(storageKey);
            _logger.Debug("Reading blob {0} from {1}", storageKey, url);
            using (var webClient = new WebClient())
            {
                var result = await webClient.DownloadDataTaskAsync(new Uri(url));
                return result;
            }
        }

        private WebRequest CreateRequest(string uri, byte[] content)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (content == null) throw new ArgumentNullException("content");

            WebRequest request = WebRequest.Create(uri);
            request.Method = "PUT";
            request.Headers.Add("x-ms-blob-type", "BlockBlob");
            request.ContentLength = content.Length;
            return request;
        }
    }
}