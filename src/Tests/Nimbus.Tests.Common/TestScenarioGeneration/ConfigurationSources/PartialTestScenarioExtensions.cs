using System.Collections.Generic;
using System.Linq;
using Nimbus.Extensions;
using NUnit.Framework;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    internal static class PartialTestScenarioExtensions
    {
        public static IEnumerable<TestCaseData> BuildTestCases<T>(this IEnumerable<PartialConfigurationScenario<T>> scenarios)
        {
            return scenarios
                .OrderBy(c => c.Name)
                .Select(c => new TestCaseData(c.Name, c.Configuration)
                            .Chain(tc =>
                                   {
                                       c.Categories
                                        .Do(category => tc.SetCategory(category))
                                        .Done();

                                       return tc;
                                   })
                );
        }
    }
}