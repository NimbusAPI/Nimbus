using Nimbus.Configuration;
using Nimbus.Ninject.Configuration;
using Ninject;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers
{
    internal class NinjectScenario : IConfigurationScenario<ContainerConfiguration>
    {
        public string Name => "Ninject";

        public string[] Categories => new[] {"Ninject"};

        public ScenarioInstance<ContainerConfiguration> CreateInstance()
        {
            var kernel = new StandardKernel();

            var configuration = new ContainerConfiguration
                                {
                                    ApplyContainerDefaults = bbc =>
                                                             {
                                                                 kernel.RegisterNimbus(bbc.TypeProvider);
                                                                 return bbc.WithNinjectDefaults(kernel);
                                                             }
                                };
            var instance = new ScenarioInstance<ContainerConfiguration>(configuration);
            instance.Disposing += (s, e) => kernel.Dispose();

            return instance;
        }
    }
}