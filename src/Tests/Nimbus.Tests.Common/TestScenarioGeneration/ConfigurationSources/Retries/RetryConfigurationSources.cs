using System.Collections;
using System.Collections.Generic;
using Nimbus.Configuration.Settings;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Retries
{
    public class RetryConfigurationSources: IEnumerable<IConfigurationScenario<RequireRetriesToBeHandledBy>>
    {
        public IEnumerator<IConfigurationScenario<RequireRetriesToBeHandledBy>> GetEnumerator()
        {
            yield return new RequireBusToHandleRetries();
            yield return new RequireTransportToHandleRetries();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}