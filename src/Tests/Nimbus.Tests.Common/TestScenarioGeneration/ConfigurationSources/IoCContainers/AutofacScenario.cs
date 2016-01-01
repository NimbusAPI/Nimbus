using Autofac;
using Nimbus.Configuration;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ConfigurationSources.IoCContainers
{
    internal class AutofacScenario : IConfigurationScenario<ContainerConfiguration>
    {
        public string Name => "Autofac";
        public string[] Categories => new[] {"Autofac"};

        public ScenarioInstance<ContainerConfiguration> CreateInstance()
        {
            var container = new ContainerBuilder().Build();

            var configuration = new ContainerConfiguration
                                {
                                    ApplyContainerDefaults = bbc =>
                                                             {
                                                                 var updater = new ContainerBuilder();
                                                                 updater.RegisterNimbus(bbc.TypeProvider);
                                                                 updater.Update(container);
                                                                 return bbc.WithAutofacDefaults(container);
                                                             }
                                };

            var instance = new ScenarioInstance<ContainerConfiguration>(configuration);
            instance.Disposing += (s, e) => container.Dispose();

            return instance;
        }
    }
}