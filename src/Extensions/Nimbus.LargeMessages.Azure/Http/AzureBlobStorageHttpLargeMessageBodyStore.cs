using System;
using System.Threading.Tasks;
using Nimbus.Configuration.LargeMessages;

namespace Nimbus.LargeMessages.Azure.Http
{
    internal class AzureBlobStorageHttpLargeMessageBodyStore : ILargeMessageBodyStore
    {
        private readonly IAzureBlobStorageHttpClient _httpClient;

        public AzureBlobStorageHttpLargeMessageBodyStore(IAzureBlobStorageHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Store(Guid id, byte[] bytes, DateTimeOffset expiresAfter)
        {
            var storageKey = DefaultStorageKeyGenerator.GenerateStorageKey(id, expiresAfter);
            await _httpClient.Upload(storageKey, bytes);
            return storageKey;
        }

        public async Task<byte[]> Retrieve(string storageKey)
        {
            return await _httpClient.Retrieve(storageKey);
        }

        public async Task Delete(string storageKey)
        {
            await _httpClient.Delete(storageKey);
        }
    }
}