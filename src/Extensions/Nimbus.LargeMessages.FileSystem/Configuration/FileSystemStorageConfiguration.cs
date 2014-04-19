using Nimbus.LargeMessages.FileSystem.Configuration.Settings;
using Nimbus.LargeMessages.FileSystem.Infrastructure;

namespace Nimbus.LargeMessages.FileSystem.Configuration
{
    public class FileSystemStorageConfiguration
    {
        internal FileSystemStorageConfiguration()
        {
        }

        internal StorageDirectorySetting StorageDirectory { get; set; }
        internal ILogger Logger { get; set; }

        public FileSystemStorageConfiguration WithStorageDirectory(string storageDirectory)
        {
            StorageDirectory = new StorageDirectorySetting {Value = storageDirectory};
            return this;
        }

        public FileSystemStorageConfiguration WithLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        public ILargeMessageBodyStore Build()
        {
            return new FileSystemLargeMessageBodyStore(StorageDirectory, Logger);
        }
    }
}