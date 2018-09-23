using Nimbus.Extensions;
using NUnit.Framework;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition
{
    internal static class PartialTestScenarioExtensions
    {
        public static TestCaseData BuildTestCase<T>(this IConfigurationScenario<T> scenario)
        {
            var testCaseData = new TestCaseData(scenario.Name, scenario);
            scenario.Categories
                    .Do(category => testCaseData.SetCategory(category))
                    .Done();

            return testCaseData;
        }
    }
}