using System.Collections;
using System.Collections.Generic;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports
{
    public class TransportConfigurationSources : IEnumerable<IConfigurationScenario<TransportConfiguration>>
    {
        public IEnumerator<IConfigurationScenario<TransportConfiguration>> GetEnumerator()
        {
            yield return new InProcess();
            yield return new Redis();

            foreach (var largeMessageStorage in new LargeMessageStorageConfigurationSources())
            {
                //yield return new WindowsServiceBus(largeMessageStorage);  //FIXME reinstate when we have separate app domains
                var configuration = new AzureServiceBus(largeMessageStorage);
                yield return configuration;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}