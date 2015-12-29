using Castle.Windsor;
using Nimbus.Configuration;
using Nimbus.Windsor.Configuration;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers
{
    internal class WindsorScenario : IConfigurationScenario<ContainerConfiguration>
    {
        public string Name { get; } = "Windsor";
        public string[] Categories { get; } = {"Windsor"};

        public ScenarioInstance<ContainerConfiguration> CreateInstance()
        {
            var windsorContainer = new WindsorContainer();

            var configuration = new ContainerConfiguration
                                {
                                    ApplyContainerDefaults = bbc =>
                                                             {
                                                                 windsorContainer.RegisterNimbus(bbc.TypeProvider);

                                                                 return bbc.WithWindsorDefaults(windsorContainer);
                                                             }
                                };

            var instance = new ScenarioInstance<ContainerConfiguration>(configuration);
            instance.Disposing += (s, e) => windsorContainer.Dispose();

            return instance;
        }
    }
}