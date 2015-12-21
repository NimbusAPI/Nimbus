using System.Collections;
using System.Collections.Generic;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using NUnit.Framework;

namespace Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources
{
    public class AllTransportConfigurations : IEnumerable<TestCaseData>
    {
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            return new TransportConfigurationSources()
                .BuildTestCases()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}