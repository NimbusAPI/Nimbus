using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.IntegrationTests.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.BusBuilder;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition.Filters;
using NUnit.Framework;

namespace Nimbus.IntegrationTests.TestScenarioGeneration.TestCaseSources
{
    public class AllBusConfigurations<TTestType> : IEnumerable<TestCaseData>
    {
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            var testFixtureType = typeof (TTestType);

            var filterAttribute = testFixtureType.GetCustomAttribute<FilterTestCasesByAttribute>();
            var filter = filterAttribute != null
                ? (IScenarioFilter) Activator.CreateInstance(filterAttribute.Type)
                : new AtLeastOneOfEachTypeOfScenarioFilter();

            var testCases = new BusBuilderConfigurationSources(testFixtureType)
                .ToArray()
                .Pipe(filter.Filter)
                .OrderBy(scenario => scenario.Name)
                .Select(scenario => scenario.BuildTestCase())
                .ToArray();

            return testCases.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}