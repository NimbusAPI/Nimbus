using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nimbus.Tests.Integration.Extensions;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.BusBuilder;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition.Filters;
using NUnit.Framework;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.TestCaseSources
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

            GuardAgainstSilentlyGeneratingNothing(testFixtureType, filter, testCases);

            return testCases.AsEnumerable().GetEnumerator();
        }

        /// <summary>
        ///     An empty scenario set produces no tests at all, and a fixture that generates no tests looks
        ///     exactly like one that passed — the run still goes green. That makes a mistargeted filter
        ///     invisible, so it fails loudly here unless the fixture has opted in to being silent.
        /// </summary>
        private static void GuardAgainstSilentlyGeneratingNothing(Type testFixtureType, IScenarioFilter filter, TestCaseData[] testCases)
        {
            if (testCases.Any()) return;
            if (testFixtureType.GetCustomAttribute<AllowNoTestCasesAttribute>() != null) return;

            throw new InvalidOperationException(
                $"{testFixtureType.Name} generated no test cases. The '{filter.GetType().Name}' filter and the " +
                $"'{TransportSelector.SelectedTransport}' transport selection (NIMBUS_TEST_TRANSPORT) have no scenarios " +
                "in common, so this fixture would silently not run. Either widen the filter, or mark the fixture with " +
                $"[{nameof(AllowNoTestCasesAttribute).Replace("Attribute", "")}(\"reason\")] if that is intended.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}