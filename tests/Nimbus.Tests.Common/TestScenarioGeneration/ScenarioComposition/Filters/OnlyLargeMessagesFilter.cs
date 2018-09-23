// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.LargeMessageStores;

// namespace Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition.Filters
// {
//     public class OnlyLargeMessagesFilter : AtLeastOneOfEachTypeOfScenarioFilter
//     {
//         private readonly Type[] _largeMessageScenarioTypes;

//         public OnlyLargeMessagesFilter()
//         {
//             _largeMessageScenarioTypes = new LargeMessageStorageConfigurationSources()
//                 .Select(scenario => scenario.GetType())
//                 .ToArray();
//         }

//         public override IEnumerable<IConfigurationScenario<T>> Filter<T>(IEnumerable<IConfigurationScenario<T>> scenarios)
//         {
//             var onlyLargeMessageScenarios = scenarios.Where(ShouldInclude).ToArray();
//             var baseResults = base.Filter(onlyLargeMessageScenarios);
//             return baseResults;
//         }

//         private bool ShouldInclude(IConfigurationScenario scenario)
//         {
//             var composedOfTypes = scenario.ComposedOf
//                                           .Select(c => c.GetType())
//                                           .ToArray();

//             return composedOfTypes
//                 .Where(component => _largeMessageScenarioTypes.Contains(component))
//                 .Any();
//         }
//     }
// }