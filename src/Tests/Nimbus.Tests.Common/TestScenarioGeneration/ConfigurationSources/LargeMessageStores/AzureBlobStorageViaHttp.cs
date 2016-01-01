using System;
using Nimbus.Configuration.LargeMessages;
using Nimbus.LargeMessages.Azure.Http;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores
{
    internal class AzureBlobStorageViaHttp : ConfigurationScenario<LargeMessageStorageConfiguration>
    {
        public override ScenarioInstance<LargeMessageStorageConfiguration> CreateInstance()
        {
            var configuration = new AzureBlobStorageHttpLargeMessageStorageConfiguration()
                .UsingBlobStorageContainer(new Uri("http://fixme.example.com"), "FIXME");

            var instance = new ScenarioInstance<LargeMessageStorageConfiguration>(configuration);

            return instance;
        }
    }
}