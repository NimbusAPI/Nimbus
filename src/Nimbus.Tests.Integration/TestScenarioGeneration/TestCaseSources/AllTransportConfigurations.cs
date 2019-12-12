using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using NUnit.Framework;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources
{
    public class AllTransportConfigurations : IEnumerable<TestCaseData>
    {
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            return new TransportConfigurationSources()
                .Select(scenario => scenario.BuildTestCase())
                .OrderBy(tc => tc.TestName)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}