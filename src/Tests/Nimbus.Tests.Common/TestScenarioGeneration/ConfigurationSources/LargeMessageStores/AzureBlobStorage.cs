using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration.LargeMessages;
using Nimbus.LargeMessages.Azure.Client;
using Nimbus.Tests.Common.Configuration;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores
{
    internal class AzureBlobStorage : IConfigurationScenario<LargeMessageStorageConfiguration>
    {
        public string Name { get; } = "AzureBlobStorage";
        public string[] Categories { get; } = {"AzureBlobStorage"};

        public ScenarioInstance<LargeMessageStorageConfiguration> CreateInstance()
        {
            var azureBlobStorageConnectionString = DefaultSettingsReader.Get<AzureBlobStorageConnectionString>();
            var configuration = new AzureBlobStorageLargeMessageStorageConfiguration()
                .UsingStorageAccountConnectionString(azureBlobStorageConnectionString);

            var instance = new ScenarioInstance<LargeMessageStorageConfiguration>(configuration);

            return instance;
        }
    }
}