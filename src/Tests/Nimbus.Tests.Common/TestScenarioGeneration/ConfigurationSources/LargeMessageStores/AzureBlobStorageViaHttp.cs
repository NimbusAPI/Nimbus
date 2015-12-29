using System;
using Nimbus.Configuration.LargeMessages;
using Nimbus.LargeMessages.Azure.Http;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores
{
    internal class AzureBlobStorageViaHttp : IConfigurationScenario<LargeMessageStorageConfiguration>
    {
        public string Name { get; } = "AzureBlobStorageViaHttp";
        public string[] Categories { get; } = {"AzureBlobStorageViaHttp"};

        public ScenarioInstance<LargeMessageStorageConfiguration> CreateInstance()
        {
            var configuration = new AzureBlobStorageHttpLargeMessageStorageConfiguration()
                .UsingBlobStorageContainer(new Uri("http://fixme.example.com"), "FIXME");

            var instance = new ScenarioInstance<LargeMessageStorageConfiguration>(configuration);

            return instance;
        }
    }
}