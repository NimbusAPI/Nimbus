using System.Collections.Generic;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.LargeMessages.FileSystem.Configuration.Settings;
using Nimbus.LargeMessages.FileSystem.Infrastructure;

namespace Nimbus.LargeMessages.FileSystem.Configuration
{
    public class FileSystemStorageConfiguration : LargeMessageStorageConfiguration
    {
        internal StorageDirectorySetting StorageDirectory { get; set; }

        public FileSystemStorageConfiguration WithStorageDirectory(string storageDirectory)
        {
            StorageDirectory = new StorageDirectorySetting {Value = storageDirectory};
            return this;
        }

        public override void Register<TLargeMessageBodyStore>(PoorMansIoC container)
        {
            container.RegisterType<FileSystemLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof (ILargeMessageBodyStore));
        }

        public override void RegisterSupportingComponents(PoorMansIoC container)
        {
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}