using System;
using Nimbus.Configuration;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources
{
    public class ContainerConfiguration
    {
        public Func<BusBuilderConfiguration, BusBuilderConfiguration> ApplyContainerDefaults { get; set; }
    }
}