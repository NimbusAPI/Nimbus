using System;
using System.Collections.Generic;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.LargeMessages.Azure.Configuration.Settings;
using Nimbus.LargeMessages.Azure.Infrastructure.Http;

namespace Nimbus.LargeMessages.Azure.Configuration
{
    public class AzureBlobStorageHttpLargeMessageStorageConfiguration : LargeMessageStorageConfiguration
    {
        internal AzureBlobStorageContainerUriSetting AzureBlobStorageContainerUri { get; set; }
        internal AzureBlobStorageContainerSharedAccessSignatureSetting AzureBlobStorageContainerSharedAccessSignature { get; set; }

        public AzureBlobStorageHttpLargeMessageStorageConfiguration UsingBlobStorageContainer(Uri containerUri, string sharedAccessSignature)
        {
            AzureBlobStorageContainerUri = new AzureBlobStorageContainerUriSetting {Value = containerUri};
            AzureBlobStorageContainerSharedAccessSignature = new AzureBlobStorageContainerSharedAccessSignatureSetting {Value = sharedAccessSignature};

            return this;
        }

        public override void Register<TLargeMessageBodyStore>(PoorMansIoC container)
        {
            container.Register(c =>
                               {
                                   var uriFormatter = new UriFormatter(AzureBlobStorageContainerUri, AzureBlobStorageContainerSharedAccessSignature);
                                   var blobStorageHttpClient = container.ResolveWithOverrides<IAzureBlobStorageHttpClient>(uriFormatter);
                                   var messageBodyStore = container.ResolveWithOverrides<AzureBlobStorageHttpLargeMessageBodyStore>(blobStorageHttpClient);
                                   return messageBodyStore;
                               },
                               ComponentLifetime.SingleInstance,
                               typeof (ILargeMessageBodyStore));
        }

        public override void RegisterSupportingComponents(PoorMansIoC container)
        {
            container.RegisterType<AzureBlobStorageHttpClient>(ComponentLifetime.SingleInstance, typeof (IAzureBlobStorageHttpClient));
            container.RegisterType<AzureBlobStorageHttpLargeMessageBodyStore>(ComponentLifetime.SingleInstance);
        }

        public override IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}