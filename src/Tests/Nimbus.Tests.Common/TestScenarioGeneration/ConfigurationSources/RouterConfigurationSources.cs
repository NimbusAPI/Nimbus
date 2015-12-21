using System.Collections;
using System.Collections.Generic;
using Nimbus.Infrastructure.Routing;
using Nimbus.Routing;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    internal class RouterConfigurationSources : IEnumerable<PartialConfigurationScenario<IRouter>>
    {
        public IEnumerator<PartialConfigurationScenario<IRouter>> GetEnumerator()
        {
            yield return
                new PartialConfigurationScenario<IRouter>(nameof(DestinationPerMessageTypeRouter),
                                                          new DestinationPerMessageTypeRouter());
            yield return
                new PartialConfigurationScenario<IRouter>(nameof(SingleQueueAndTopicPerMessageTypeRouter),
                                                          new SingleQueueAndTopicPerMessageTypeRouter(new DestinationPerMessageTypeRouter()));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}