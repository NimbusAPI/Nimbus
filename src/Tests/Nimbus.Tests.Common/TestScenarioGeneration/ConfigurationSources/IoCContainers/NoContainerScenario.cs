using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers
{
    public class NoContainerScenario : IConfigurationScenario<ContainerConfiguration>
    {
        public string Name { get; } = "NoContainer";
        public string[] Categories { get; } = {"NoContainer"};

        public ScenarioInstance<ContainerConfiguration> CreateInstance()
        {
            var configuration = new ContainerConfiguration
                                {
                                    ApplyContainerDefaults = bbc => bbc.WithDependencyResolver(new DependencyResolver(bbc.TypeProvider))
                                };

            var instance = new ScenarioInstance<ContainerConfiguration>(configuration);

            return instance;
        }
    }
}