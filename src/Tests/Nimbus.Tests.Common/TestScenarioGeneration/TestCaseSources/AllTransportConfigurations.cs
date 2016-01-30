using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using NUnit.Framework;

namespace Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources
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