using ConfigInjector.QuickAndDirty;
using Nimbus.Configuration.LargeMessages;
using Nimbus.LargeMessages.Azure.Http;
using Nimbus.Tests.Common.Configuration;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores
{
    internal class AzureBlobStorageViaHttp : ConfigurationScenario<LargeMessageStorageConfiguration>
    {
        public override ScenarioInstance<LargeMessageStorageConfiguration> CreateInstance()
        {
            var uri = DefaultSettingsReader.Get<AzureBlobStorageContainerUri>();
            var accessKey = DefaultSettingsReader.Get<AzureBlobStorageContainerSharedAccessSignature>();

            var configuration = new AzureBlobStorageHttpLargeMessageStorageConfiguration()
                .UsingBlobStorageContainer(uri, accessKey);

            var instance = new ScenarioInstance<LargeMessageStorageConfiguration>(configuration);

            return instance;
        }
    }
}