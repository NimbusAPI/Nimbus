using System.Collections;
using System.Collections.Generic;
using Nimbus.Routing;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Routers
{
    internal class RouterConfigurationSources : IEnumerable<IConfigurationScenario<IRouter>>
    {
        public IEnumerator<IConfigurationScenario<IRouter>> GetEnumerator()
        {
            yield return new DestinationPerMessageType();
            yield return new SingleQueueAndTopicPerMessageType();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}