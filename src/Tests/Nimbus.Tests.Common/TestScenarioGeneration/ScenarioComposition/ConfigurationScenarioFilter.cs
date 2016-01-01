using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Tests.Common.Extensions;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers;
using Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.Transports;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    public class ConfigurationScenarioFilter
    {
        private readonly HashSet<Type> _previouslySeenScenarios = new HashSet<Type>();

        private readonly Type[] _alwaysInclude = {typeof (InProcess), typeof (NoContainerScenario)};

        public bool ShouldInclude(IConfigurationScenario scenario)
        {
            var composingScenarios = scenario.ComposedOf.ToArray();
            if (composingScenarios.Any(s => _alwaysInclude.Contains(s.GetType()))) return true;
            if (composingScenarios.All(s => _previouslySeenScenarios.Contains(s.GetType()))) return false;

            _previouslySeenScenarios.AddRange(composingScenarios.Select(s => s.GetType()));
            return true;
        }
    }
}