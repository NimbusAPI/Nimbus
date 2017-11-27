using System.Collections.Generic;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.LargeMessages.Azure.Configuration.Settings;

namespace Nimbus.LargeMessages.Azure.Client
{
    public class AzureBlobStorageLargeMessageStorageConfiguration : LargeMessageStorageConfiguration
    {
        internal AzureStorageAccountConnectionStringSetting AzureStorageAccountConnectionString { get; set; }
        internal AutoCreateBlobStorageContainerNameSetting AutoCreateBlobStorageContainerName { get; set; } = new AutoCreateBlobStorageContainerNameSetting();

        public AzureBlobStorageLargeMessageStorageConfiguration UsingStorageAccountConnectionString(string connectionString)
        {
            AzureStorageAccountConnectionString = new AzureStorageAccountConnectionStringSetting {Value = connectionString};
            return this;
        }

        public AzureBlobStorageLargeMessageStorageConfiguration UsingBlobStorageContainerName(string containerName)
        {
            AutoCreateBlobStorageContainerName = new AutoCreateBlobStorageContainerNameSetting {Value = containerName};
            return this;
        }

        public override void Register<TLargeMessageBodyStore>(PoorMansIoC container)
        {
            container.RegisterType<AzureBlobStorageLargeMessageBodyStore>(ComponentLifetime.SingleInstance, typeof (ILargeMessageBodyStore));
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