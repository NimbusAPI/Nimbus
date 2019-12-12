using System;
using Nimbus.Configuration;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers
{
    public class ContainerConfiguration
    {
        public Func<BusBuilderConfiguration, BusBuilderConfiguration> ApplyContainerDefaults { get; set; }
    }
}