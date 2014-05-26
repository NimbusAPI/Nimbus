using System;
using Nimbus.LargeMessages.Azure.Configuration.Settings;
using Nimbus.LargeMessages.Azure.Infrastructure.RestApi;

namespace Nimbus.LargeMessages.Azure.Infrastructure
{
    public class BlobStorageConfiguration
    {
        internal BlobStorageConnectionStringSetting BlobStorageConnectionString { get; set; }
        internal RestStorageUrlSetting RestStorageUrl { get; set; }
        internal RestStorageSharedAccessKeySetting RestStorageSharedAccessKey { get; set; }
        internal ILogger Logger { get; set; }
        internal bool UseRestStorage { get; set; }

        public BlobStorageConfiguration WithBlobStorageConnectionString(string connectionString)
        {
            if(UseRestStorage)
                throw new InvalidOperationException("You are trying to configure blob storage, however you have already configured to use REST blob storage which will override this setting.");

            BlobStorageConnectionString = new BlobStorageConnectionStringSetting { Value = connectionString };
            return this;
        }

        public BlobStorageConfiguration WithRestfulStorage(string containerUrl, string sharedAccessKey)
        {
            RestStorageUrl = new RestStorageUrlSetting() { Value = containerUrl };
            RestStorageSharedAccessKey = new RestStorageSharedAccessKeySetting() { Value = sharedAccessKey };
            UseRestStorage = true;
            return this;
        }

        public BlobStorageConfiguration WithLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        public ILargeMessageBodyStore Build()
        {
            if (UseRestStorage)
                return new AzureRestApiBlobStorageLargeMessageBodyStore(new RestApiHelper(new UrlFormatter(RestStorageUrl, RestStorageSharedAccessKey), Logger));
            return new AzureBlobStorageLargeMessageBodyStore(BlobStorageConnectionString, Logger);
        }
    }
}