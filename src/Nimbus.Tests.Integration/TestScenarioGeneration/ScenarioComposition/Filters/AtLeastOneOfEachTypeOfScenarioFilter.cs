using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Tests.Integration.Extensions;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.IoCContainers;
using Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.Transports;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition.Filters
{
    public class AtLeastOneOfEachTypeOfScenarioFilter : IScenarioFilter
    {
        private readonly Type[] _alwaysInclude = {typeof (InProcess), typeof (NoContainerScenario)};

        public virtual IEnumerable<IConfigurationScenario<T>> Filter<T>(IEnumerable<IConfigurationScenario<T>> scenarios)
        {
            var allScenarios = scenarios.ToArray();
            var mandatoryScenarios = allScenarios
                .Where(s => s.ComposedOf.Any(c => _alwaysInclude.Contains(c.GetType())))
                .ToArray();
            var nonMandatoryScenarios = allScenarios.Except(mandatoryScenarios).ToArray();

            var selectedScenarios = new List<IConfigurationScenario<T>>();

            var previouslySeenScenarioTypes = new HashSet<Type>();
            previouslySeenScenarioTypes.AddRange(_alwaysInclude);

            while (true)
            {
                var nextScenario = CombinationWithGreatestNumberOfUnseenScenarios(nonMandatoryScenarios, previouslySeenScenarioTypes);
                if (nextScenario == null) break;

                previouslySeenScenarioTypes.AddRange(nextScenario.ComposedOf.Select(c => c.GetType()));
                selectedScenarios.Add(nextScenario);
            }

            var filteredScenarios = new List<IConfigurationScenario<T>>();
            filteredScenarios.AddRange(mandatoryScenarios);
            filteredScenarios.AddRange(selectedScenarios);
            return filteredScenarios;
        }

        private static IConfigurationScenario<T> CombinationWithGreatestNumberOfUnseenScenarios<T>(IConfigurationScenario<T>[] allScenarios,
                                                                                                   HashSet<Type> previouslySeenScenarioTypes)
        {
            var result = allScenarios
                .OrderByDescending(s => ComposedOfHowManyUnseenScenarios(previouslySeenScenarioTypes, s))
                .Where(s => ComposedOfHowManyUnseenScenarios(previouslySeenScenarioTypes, s) > 0)
                .FirstOrDefault();
            return result;
        }

        private static int ComposedOfHowManyUnseenScenarios<T>(HashSet<Type> previouslySeenScenarioTypes, IConfigurationScenario<T> s)
        {
            return s.ComposedOf
                    .Where(c => !previouslySeenScenarioTypes.Contains(c.GetType()))
                    .Count();
        }
    }
}