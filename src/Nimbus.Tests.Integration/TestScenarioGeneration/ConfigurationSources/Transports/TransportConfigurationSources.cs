using System.Collections;
using System.Collections.Generic;
using Nimbus.Configuration.Transport;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports
{
    public class TransportConfigurationSources : IEnumerable<IConfigurationScenario<TransportConfiguration>>
    {
        public IEnumerator<IConfigurationScenario<TransportConfiguration>> GetEnumerator()
        {
            yield return new InProcess();
            yield return new Redis();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}