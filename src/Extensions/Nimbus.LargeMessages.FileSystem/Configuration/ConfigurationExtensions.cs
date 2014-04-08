using System;
using Nimbus.Configuration;

namespace Nimbus.LargeMessages.FileSystem.Configuration
{
    public static class ConfigurationExtensions
    {
        public static BusBuilderConfiguration WithFileSystemMessageBodyStorage(this BusBuilderConfiguration configuration,
                                                                               Action<FileSystemStorageConfiguration> configurationAction)
        {
            var fileSystemStorageConfiguration = new FileSystemStorageConfiguration(configuration);
            configurationAction(fileSystemStorageConfiguration);
            configuration.LargeMessageBodyConfiguration = fileSystemStorageConfiguration;
            return configuration;
        }
    }
}