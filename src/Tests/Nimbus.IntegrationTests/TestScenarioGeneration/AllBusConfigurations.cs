using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.TestScenarioGeneration
{
    public class AllBusConfigurations<TTestType> : IEnumerable<TestCaseData>
    {
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            return new BusBuilderConfigurationSources(typeof (TTestType))
                .OrderBy(c => c.Name)
                .Select(c => new TestCaseData(c.Name, c.Configuration))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}