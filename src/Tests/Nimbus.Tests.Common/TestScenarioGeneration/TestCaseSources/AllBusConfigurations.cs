using System.Collections;
using System.Collections.Generic;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources;
using NUnit.Framework;

namespace Nimbus.Tests.Common.TestScenarioGeneration.TestCaseSources
{
    public class AllBusConfigurations<TTestType> : IEnumerable<TestCaseData>
    {
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            return new BusBuilderConfigurationSources(typeof (TTestType))
                .BuildTestCases()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}