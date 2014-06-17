using System;
using Nimbus.LargeMessages.Azure.Configuration.Settings;
using Nimbus.LargeMessages.Azure.Infrastructure;
using Nimbus.LargeMessages.Azure.Infrastructure.Http;

namespace Nimbus.LargeMessages.Azure.Configuration
{
    public class BlobStorageConfiguration
    {
        internal AzureStorageAccountConnectionStringSetting AzureStorageAccountConnectionString { get; set; }
        internal AutoCreateBlobStorageContainerNameSetting AutoCreateBlobStorageContainerName { get; set; }
        internal AzureBlobStorageContainerUriSetting AzureBlobStorageContainerUri { get; set; }
        internal AzureBlobStorageContainerSharedAccessSignatureSetting AzureBlobStorageContainerSharedAccessSignature { get; set; }
        internal ILogger Logger { get; set; }

        private Func<ILogger, ILargeMessageBodyStore> _storeBuilder = logger =>
        {
            throw new InvalidOperationException("You are trying to build the large message store, but you have not configured the store access method to use.");
        };

        public BlobStorageConfiguration UsingStorageAccountConnectionString(string connectionString, string autoCreateBlobStorageContainerName = "messagebodies")
        {
            if (AzureBlobStorageContainerUri != null)
                throw new InvalidOperationException("You have already configured the Azure large message feature to use a specific Container and Shared Access Signature. There is no need to provide a Storage Account Connection String.");

            AzureStorageAccountConnectionString = new AzureStorageAccountConnectionStringSetting {Value = connectionString};
            AutoCreateBlobStorageContainerName = new AutoCreateBlobStorageContainerNameSetting {Value = autoCreateBlobStorageContainerName};
            _storeBuilder = logger => new AzureBlobStorageLargeMessageBodyStore(AzureStorageAccountConnectionString, AutoCreateBlobStorageContainerName, logger);
            return this;
        }

        public BlobStorageConfiguration UsingBlobStorageContainer(Uri containerUri, string sharedAccessSignature)
        {
            if (AzureStorageAccountConnectionString != null)
                throw new InvalidOperationException("You have already configured the Azure large message feature to use a Storage Account Connection String. There is no need to provide a specific Container URI and Shared Access Signature.");

            AzureBlobStorageContainerUri = new AzureBlobStorageContainerUriSetting { Value = containerUri };
            AzureBlobStorageContainerSharedAccessSignature = new AzureBlobStorageContainerSharedAccessSignatureSetting { Value = sharedAccessSignature };
            _storeBuilder = logger => new AzureBlobStorageHttpLargeMessageBodyStore(new AzureBlobStorageHttpClient(new UriFormatter(AzureBlobStorageContainerUri, AzureBlobStorageContainerSharedAccessSignature), logger));
            return this;
        }

        public BlobStorageConfiguration WithLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        public ILargeMessageBodyStore Build()
        {
            return _storeBuilder(Logger);
        }
    }
}