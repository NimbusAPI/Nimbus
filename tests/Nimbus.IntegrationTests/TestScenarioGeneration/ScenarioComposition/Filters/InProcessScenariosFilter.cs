using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition.Filters
{
    public class InProcessScenariosFilter : IScenarioFilter
    {
        private readonly Type[] _alwaysInclude = {typeof (InProcess)};

        public IEnumerable<IConfigurationScenario<T>> Filter<T>(IEnumerable<IConfigurationScenario<T>> scenarios)
        {
            var allScenarios = scenarios.ToArray();
            var mandatoryScenarios = allScenarios
                .Where(s => s.ComposedOf.Any(c => _alwaysInclude.Contains(c.GetType())))
                .ToArray();
            return mandatoryScenarios;
        }
    }
}