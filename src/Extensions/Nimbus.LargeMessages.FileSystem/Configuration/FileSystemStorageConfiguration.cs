using Nimbus.Configuration;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages;
using Nimbus.LargeMessages.FileSystem.Configuration.Settings;
using Nimbus.LargeMessages.FileSystem.Infrastructure;

namespace Nimbus.LargeMessages.FileSystem.Configuration
{
    public class FileSystemStorageConfiguration : LargeMessageStorageConfiguration
    {
        private readonly BusBuilderConfiguration _configuration;

        public FileSystemStorageConfiguration(BusBuilderConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal StorageDirectorySetting StorageDirectory { get; set; }

        public FileSystemStorageConfiguration WithStorageDirectory(string storageDirectory)
        {
            StorageDirectory = new StorageDirectorySetting {Value = storageDirectory};
            return this;
        }

        public override ILargeMessageBodyStore LargeMessageBodyStore
        {
            get { return new FileSystemLargeMessageBodyStore(StorageDirectory, _configuration.Logger); }
        }
    }
}