using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration.LargeMessages;
using Nimbus.LargeMessages.Azure.Client;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores
{
    internal class AzureBlobStorage : ConfigurationScenario<LargeMessageStorageConfiguration>
    {
        public override ScenarioInstance<LargeMessageStorageConfiguration> CreateInstance()
        {
            var azureBlobStorageConnectionString = DefaultSettingsReader.Get<AzureBlobStorageConnectionString>();
            var configuration = new AzureBlobStorageLargeMessageStorageConfiguration()
                .UsingStorageAccountConnectionString(azureBlobStorageConnectionString);

            var instance = new ScenarioInstance<LargeMessageStorageConfiguration>(configuration);

            return instance;
        }
    }
}