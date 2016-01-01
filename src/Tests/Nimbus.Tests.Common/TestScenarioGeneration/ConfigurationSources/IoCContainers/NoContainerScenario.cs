using System.Collections.Generic;
using Nimbus.Configuration;
using Nimbus.Infrastructure.DependencyResolution;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers
{
    public class NoContainerScenario : ConfigurationScenario<ContainerConfiguration>
    {
        protected override IEnumerable<string> AdditionalCategories
        {
            get { yield return "SmokeTest"; }
        }

        public override ScenarioInstance<ContainerConfiguration> CreateInstance()
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