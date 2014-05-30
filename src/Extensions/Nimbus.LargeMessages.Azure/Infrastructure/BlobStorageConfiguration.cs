using System;
using Nimbus.LargeMessages.Azure.Configuration.Settings;
using Nimbus.LargeMessages.Azure.Infrastructure.RestApi;

namespace Nimbus.LargeMessages.Azure.Infrastructure
{
    public class BlobStorageConfiguration
    {
        internal BlobStorageConnectionStringSetting BlobStorageConnectionString { get; set; }
        internal RestStorageUriSetting RestStorageUri { get; set; }
        internal RestStorageSharedAccessKeySetting RestStorageSharedAccessSignature { get; set; }
        internal ILogger Logger { get; set; }

        private Func<ILogger, ILargeMessageBodyStore> _storeBuilder = (logger) =>
        {
            throw new InvalidOperationException("You are trying to build the large message store, but you have not configured the store access method.");
        };

        public BlobStorageConfiguration WithBlobStorageConnectionString(string connectionString)
        {
            if (RestStorageUri != null)
                throw new InvalidOperationException("You are trying to configure blob storage, however you have already configured to use the rest api blob storage which will override this setting.");

            BlobStorageConnectionString = new BlobStorageConnectionStringSetting { Value = connectionString };
            _storeBuilder = (logger) => new AzureBlobStorageLargeMessageBodyStore(BlobStorageConnectionString, logger);
            return this;
        }

        public BlobStorageConfiguration WithRestfulStorage(string containerUri, string sharedAccessSignature)
        {
            if (BlobStorageConnectionString != null)
                throw new InvalidOperationException("You are trying to configure the rest api storage, however you have already configured to use blob storage which will override this setting.");

            RestStorageUri = new RestStorageUriSetting() { Value = containerUri };
            RestStorageSharedAccessSignature = new RestStorageSharedAccessKeySetting() { Value = sharedAccessSignature };
            _storeBuilder = (logger) => new AzureRestApiBlobStorageLargeMessageBodyStore(new RestApiHelper(new UriFormatter(RestStorageUri, RestStorageSharedAccessSignature), logger));
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