using System;
using Nimbus.Configuration;
using Nimbus.LargeMessages.Azure.Infrastructure;

namespace Nimbus.LargeMessages.Azure.Configuration
{
    public static class ConfigurationExtensions
    {
        public static BusBuilderConfiguration WithAzureMessageBodyStorage(this BusBuilderConfiguration configuration, Action<BlobStorageConfiguration> configurationAction)
        {
            var blobStorageConfiguration = new BlobStorageConfiguration(configuration);
            configuration.LargeMessageStorageConfiguration = blobStorageConfiguration;
            configurationAction(blobStorageConfiguration);
            return configuration;
        }
    }
}