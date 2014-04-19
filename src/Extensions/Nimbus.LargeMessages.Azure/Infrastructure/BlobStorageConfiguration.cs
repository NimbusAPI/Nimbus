using Nimbus.LargeMessages.Azure.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Infrastructure
{
    public class BlobStorageConfiguration
    {
        internal BlobStorageConnectionStringSetting BlobStorageConnectionString { get; set; }
        internal ILogger Logger { get; set; }

        public BlobStorageConfiguration WithBlobStorageConnectionString(string connectionString)
        {
            BlobStorageConnectionString = new BlobStorageConnectionStringSetting {Value = connectionString};
            return this;
        }

        public BlobStorageConfiguration WithLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        public ILargeMessageBodyStore Build()
        {
            return new AzureBlobStorageLargeMessageBodyStore(BlobStorageConnectionString, Logger);
        }
    }
}