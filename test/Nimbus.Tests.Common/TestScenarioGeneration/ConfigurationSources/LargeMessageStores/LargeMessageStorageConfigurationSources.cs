using System.Collections;
using System.Collections.Generic;
using Nimbus.Configuration.LargeMessages;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores
{
    public class LargeMessageStorageConfigurationSources : IEnumerable<IConfigurationScenario<LargeMessageStorageConfiguration>>
    {
        public IEnumerator<IConfigurationScenario<LargeMessageStorageConfiguration>> GetEnumerator()
        {
            yield return new FileSystem();
            yield return new AzureBlobStorage();

            //FIXME this one needs some love.
            //yield return new AzureBlobStorageViaHttp();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}