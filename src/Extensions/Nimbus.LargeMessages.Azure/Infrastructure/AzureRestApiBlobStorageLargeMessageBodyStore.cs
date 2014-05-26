using System;
using System.Threading.Tasks;
using Nimbus.Configuration.LargeMessages;
using Nimbus.LargeMessages.Azure.Infrastructure.RestApi;

namespace Nimbus.LargeMessages.Azure.Infrastructure
{
    internal class AzureRestApiBlobStorageLargeMessageBodyStore : ILargeMessageBodyStore
    {
        private readonly IRestApiHelper _restApi;

        public AzureRestApiBlobStorageLargeMessageBodyStore(IRestApiHelper restApi)
        {
            _restApi = restApi;
        }

        public async Task<string> Store(string id, byte[] bytes, DateTimeOffset expiresAfter)
        {
            string storageKey = DefaultStorageKeyGenerator.GenerateStorageKey(id, expiresAfter);

            await _restApi.Upload(storageKey, bytes);

            return storageKey;
        }

        public async Task<byte[]> Retrieve(string storageKey)
        {
            return await _restApi.Retrieve(storageKey);
        }

        public async Task Delete(string storageKey)
        {
            await _restApi.Delete(storageKey);
        }
    }
}