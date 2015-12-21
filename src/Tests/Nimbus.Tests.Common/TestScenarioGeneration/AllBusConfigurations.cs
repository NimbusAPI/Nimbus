using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Extensions;
using NUnit.Framework;

namespace Nimbus.Tests.Common.TestScenarioGeneration
{
    public class AllBusConfigurations<TTestType> : IEnumerable<TestCaseData>
    {
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            return new BusBuilderConfigurationSources(typeof (TTestType))
                .OrderBy(c => c.Name)
                .Select(c => new TestCaseData(c.Name, c.Configuration)
                            .Chain(tc =>
                                   {
                                       c.Categories
                                        .Do(category => tc.SetCategory(category))
                                        .Done();

                                       return tc;
                                   })
                )
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}