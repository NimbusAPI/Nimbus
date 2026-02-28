using Autofac;
using Nimbus.Configuration;
using Nimbus.Tests.Integration.TestScenarioGeneration.ScenarioComposition;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources.IoCContainers
{
    internal class AutofacScenario : ConfigurationScenario<ContainerConfiguration>
    {
        public override ScenarioInstance<ContainerConfiguration> CreateInstance()
        {
            var container = new ContainerBuilder().Build();

            var configuration = new ContainerConfiguration
                                {
                                    ApplyContainerDefaults = bbc =>
                                                             {
                                                                 var updater = new ContainerBuilder();
                                                                 updater.RegisterNimbus(bbc.TypeProvider);
                                                                 //updater.Update(container);
                                                                 return bbc.WithAutofacDefaults(container);
                                                             }
                                };

            var instance = new ScenarioInstance<ContainerConfiguration>(configuration);
            instance.Disposing += (s, e) => container.Dispose();

            return instance;
        }
    }
}