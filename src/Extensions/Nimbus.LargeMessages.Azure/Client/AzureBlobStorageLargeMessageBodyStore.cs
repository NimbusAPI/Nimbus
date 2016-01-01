using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.LargeMessages;
using Nimbus.LargeMessages.Azure.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Client
{
    internal class AzureBlobStorageLargeMessageBodyStore : ILargeMessageBodyStore
    {
        private readonly AzureStorageAccountConnectionStringSetting _azureStorageAccountConnectionString;
        private readonly AutoCreateBlobStorageContainerNameSetting _autoCreateBlobStorageContainerName;
        private readonly ILogger _logger;

        private readonly ThreadSafeLazy<CloudStorageAccount> _storageAccount;
        private readonly ThreadSafeLazy<CloudBlobClient> _blobClient;
        private readonly ThreadSafeLazy<CloudBlobContainer> _container;

        internal AzureBlobStorageLargeMessageBodyStore(
            AzureStorageAccountConnectionStringSetting azureStorageAccountConnectionString,
            AutoCreateBlobStorageContainerNameSetting autoCreateBlobStorageContainerName,
            ILogger logger)
        {
            _azureStorageAccountConnectionString = azureStorageAccountConnectionString;
            _autoCreateBlobStorageContainerName = autoCreateBlobStorageContainerName;
            _logger = logger;

            _storageAccount = new ThreadSafeLazy<CloudStorageAccount>(OpenCloudStorageAccount);
            _blobClient = new ThreadSafeLazy<CloudBlobClient>(CreateCloudBlobClient);
            _container = new ThreadSafeLazy<CloudBlobContainer>(GetContainerReference);
        }

        public async Task<string> Store(Guid id, byte[] bytes, DateTimeOffset expiresAfter)
        {
            var storageKey = DefaultStorageKeyGenerator.GenerateStorageKey(id, expiresAfter);
            var blobReference = _container.Value.GetBlockBlobReference(storageKey);
            _logger.Debug("Writing blob {0} to {1}", id, blobReference.Uri);

            await blobReference.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
            return storageKey;
        }

        public async Task<byte[]> Retrieve(string id)
        {
            var blobReference = _container.Value.GetBlockBlobReference(id);
            _logger.Debug("Reading blob {0} from {1}", id, blobReference.Uri);

            using (var ms = new MemoryStream())
            {
                await blobReference.DownloadToStreamAsync(ms);
                var bytes = ms.GetBuffer().Take((int) ms.Length).ToArray();
                return bytes;
            }
        }

        public async Task Delete(string id)
        {
            var blobReference = _container.Value.GetBlockBlobReference(id);
            _logger.Debug("Deleting blob {0} from {1}", id, blobReference.Uri);
            await blobReference.DeleteAsync();
        }

        private CloudStorageAccount OpenCloudStorageAccount()
        {
            return CloudStorageAccount.Parse(_azureStorageAccountConnectionString);
        }

        private CloudBlobClient CreateCloudBlobClient()
        {
            return _storageAccount.Value.CreateCloudBlobClient();
        }

        private CloudBlobContainer GetContainerReference()
        {
            var cloudBlobContainer = _blobClient.Value.GetContainerReference(_autoCreateBlobStorageContainerName);
            cloudBlobContainer.CreateIfNotExists(BlobContainerPublicAccessType.Blob);
            return cloudBlobContainer;
        }
    }
}