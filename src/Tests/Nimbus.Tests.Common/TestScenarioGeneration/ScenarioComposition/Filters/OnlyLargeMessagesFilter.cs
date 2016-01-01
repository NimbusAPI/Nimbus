using System;
using System.Linq;
using Nimbus.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition.Filters
{
    public class OnlyLargeMessagesFilter : AtLeastOneOfEachTypeOfScenarioFilter
    {
        private readonly Type[] _largeMessageScenarioTypes;

        public OnlyLargeMessagesFilter()
        {
            _largeMessageScenarioTypes = new LargeMessageStorageConfigurationSources()
                .Select(scenario => scenario.GetType())
                .ToArray();
        }

        public override bool ShouldInclude(IConfigurationScenario scenario)
        {
            var composedOfTypes = scenario.ComposedOf
                                          .Select(c => c.GetType())
                                          .ToArray();

            if (composedOfTypes.None(component => _largeMessageScenarioTypes.Contains(component))) return false;

            return base.ShouldInclude(scenario);
        }
    }
}