using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Nimbus.Configuration.LargeMessages;
using Nimbus.InfrastructureContracts;
using Nimbus.LargeMessages.Azure.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Client
{
    internal class AzureBlobStorageLargeMessageBodyStore : ILargeMessageBodyStore
    {
        private readonly AzureStorageAccountConnectionStringSetting _azureStorageAccountConnectionString;
        private readonly AutoCreateBlobStorageContainerNameSetting _autoCreateBlobStorageContainerName;
        private readonly ILogger _logger;

        internal AzureBlobStorageLargeMessageBodyStore(
            AzureStorageAccountConnectionStringSetting azureStorageAccountConnectionString,
            AutoCreateBlobStorageContainerNameSetting autoCreateBlobStorageContainerName,
            ILogger logger)
        {
            _azureStorageAccountConnectionString = azureStorageAccountConnectionString;
            _autoCreateBlobStorageContainerName = autoCreateBlobStorageContainerName;
            _logger = logger;
        }

        private async Task<BlobContainerClient> GetContainerClient()
        {
            var blobClient = new BlobServiceClient(_azureStorageAccountConnectionString);
            var containerClient = blobClient.GetBlobContainerClient(_autoCreateBlobStorageContainerName);
            await containerClient.CreateIfNotExistsAsync();
            return containerClient;
        }
        
        public async Task<string> Store(Guid id, byte[] bytes, DateTimeOffset expiresAfter)
        {
            var storageKey = DefaultStorageKeyGenerator.GenerateStorageKey(id, expiresAfter);
            var containerClient = await GetContainerClient();
            var blobClient = containerClient.GetBlobClient(storageKey);
            _logger.Debug("Writing blob {0} to {1}", id, blobClient.Uri);

            var content = new BinaryData(bytes);
            var info = await blobClient.UploadAsync(content);
            return storageKey;
        }

        public async Task<byte[]> Retrieve(string id)
        {
            var containerClient = await GetContainerClient();
            var blobClient = containerClient.GetBlobClient(id);
            _logger.Debug("Reading blob {0} from {1}", id, blobClient.Uri);

            await using var ms = new MemoryStream();
            await blobClient.DownloadToAsync(ms);
            var bytes = ms.GetBuffer().Take((int) ms.Length).ToArray();
            return bytes;
        }

        public async Task Delete(string id)
        {
            var containerClient = await GetContainerClient();
            var blobClient = containerClient.GetBlobClient(id);
            _logger.Debug("Deleting blob {0} from {1}", id, blobClient.Uri);
            await blobClient.DeleteAsync();
        }

    }
}