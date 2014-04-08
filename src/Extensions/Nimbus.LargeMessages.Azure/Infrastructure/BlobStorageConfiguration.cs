using Nimbus.Configuration;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.LargeMessages.Azure.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Infrastructure
{
    public class BlobStorageConfiguration : LargeMessageStorageConfiguration
    {
        private readonly BusBuilderConfiguration _configuration;

        internal BlobStorageConfiguration(BusBuilderConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal BlobStorageConnectionStringSetting BlobStorageConnectionString { get; set; }

        public BlobStorageConfiguration WithBlobStorageConnectionString(string connectionString)
        {
            BlobStorageConnectionString = new BlobStorageConnectionStringSetting {Value = connectionString};
            return this;
        }

        public override ILargeMessageBodyStore LargeMessageBodyStore
        {
            get { return new AzureBlobStorageLargeMessageBodyStore(BlobStorageConnectionString, _configuration.Logger); }
        }
    }
}